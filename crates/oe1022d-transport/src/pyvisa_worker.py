import base64
import json
import sys
import time

import pyvisa
from pyvisa import constants


rm = None
inst = None


def resource_manager():
    global rm
    if rm is None:
        rm = pyvisa.ResourceManager()
    return rm


def close_inst():
    global inst
    if inst is not None:
        try:
            inst.close()
        finally:
            inst = None


def write_response(request_id, ok, result=None, error=None, error_kind=None, partial_len=None):
    response = {"id": request_id, "ok": ok}
    if ok:
        response["result"] = result
    else:
        response["error"] = str(error)
        if error_kind is not None:
            response["error_kind"] = error_kind
        if partial_len is not None:
            response["partial_len"] = partial_len
    sys.stdout.write(json.dumps(response, separators=(",", ":")) + "\n")
    sys.stdout.flush()


def classify_error(exc):
    text = str(exc)
    if "VI_ERROR_TMO" in text or "timeout" in text.lower():
        return "timeout"
    return "io"


def open_resource(resource, baud_rate, timeout_ms):
    global inst
    close_inst()
    inst = resource_manager().open_resource(resource)
    inst.timeout = int(timeout_ms)
    inst.chunk_size = 12288
    inst.baud_rate = int(baud_rate)
    inst.data_bits = 8
    inst.parity = constants.Parity.none
    inst.stop_bits = constants.StopBits.one
    inst.flow_control = constants.VI_ASRL_FLOW_NONE
    inst.write_termination = None
    inst.read_termination = None
    try:
        inst.set_visa_attribute(constants.VI_ATTR_TERMCHAR_EN, constants.VI_FALSE)
    except Exception:
        pass
    clear_resource()


def clear_resource():
    if inst is None:
        raise RuntimeError("resource is not open")
    try:
        inst.clear()
    except Exception:
        pass
    try:
        inst.flush(constants.VI_READ_BUF_DISCARD | constants.VI_WRITE_BUF_DISCARD)
    except Exception:
        pass


def write_command(command):
    if inst is None:
        raise RuntimeError("resource is not open")
    inst.write_raw(command.encode("ascii") + b"\r")


def read_ascii_line(max_bytes):
    if inst is None:
        raise RuntimeError("resource is not open")
    out = bytearray()
    while len(out) <= max_bytes:
        chunk = inst.read_bytes(1, chunk_size=1, break_on_termchar=False)
        if not chunk:
            if not out:
                raise TimeoutError("ASCII response timeout")
            break
        byte = chunk[0]
        if byte in (10, 13):
            break
        out.append(byte)
    if len(out) > max_bytes:
        raise RuntimeError(f"ASCII response exceeds {max_bytes} bytes")
    return out.decode("ascii", errors="replace").strip()


def query_text(command, max_bytes):
    write_command(command)
    return read_ascii_line(max_bytes)


def query_rall_exact(expected_len, post_write_delay_ms):
    if inst is None:
        raise RuntimeError("resource is not open")
    inst.write_raw(b"RALL?\r")
    time.sleep(float(post_write_delay_ms) / 1000.0)
    payload = inst.read_bytes(
        int(expected_len),
        chunk_size=int(expected_len),
        break_on_termchar=False,
    )
    if len(payload) != int(expected_len):
        raise RuntimeError(f"RALL payload length mismatch: {len(payload)} != {expected_len}")
    return base64.b64encode(payload).decode("ascii")


def handle(request):
    op = request.get("op")
    if op == "list_resources":
        return list(resource_manager().list_resources())
    if op == "open":
        open_resource(request["resource"], request["baud_rate"], request["timeout_ms"])
        return {"resource": request["resource"]}
    if op == "clear":
        clear_resource()
        return None
    if op == "send":
        write_command(request["command"])
        return None
    if op == "query_text":
        return query_text(request["command"], int(request.get("max_response_bytes", 4096)))
    if op == "query_rall_exact":
        return query_rall_exact(request["expected_len"], request["post_write_delay_ms"])
    raise RuntimeError(f"unknown op: {op}")


def main():
    for line in sys.stdin:
        if not line.strip():
            continue
        request_id = None
        try:
            request = json.loads(line)
            request_id = request.get("id")
            result = handle(request)
            write_response(request_id, True, result=result)
        except Exception as exc:
            error_kind = classify_error(exc)
            partial_len = 0 if error_kind == "timeout" else None
            write_response(request_id, False, error=exc, error_kind=error_kind, partial_len=partial_len)


if __name__ == "__main__":
    main()
