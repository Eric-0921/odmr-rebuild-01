export const defaultPlanTemplate = Object.freeze({
  run_id: "generated_plan",
  operator: "local",
  acquisition_window_ms: 0,
  point_settle_ms: 500,
  failure_policy: "continue",
  mag_baseline_policy: {
    baseline_current_a: [0.0, 0.0, 0.0],
    settle_ms: 1000,
    readback_samples: 3,
    settle_tolerance_a: 0.002,
    voltage_v: 75.0,
    voltage_protection_v: 75.0,
    output_enabled: true
  },
  quality_thresholds: {
    min_frames: 20,
    max_timeout_count: 2,
    max_duplicate_ratio: 0.3,
    max_last_frame_age_ms: 500
  },
  points: []
});

export function defaultBlock(prefix = "x_line") {
  return {
    prefix,
    traversal: "raster",
    totalPoints: 0,
    axes: {
      x: rangeAxis(true, 0, 40, 10, 0),
      y: rangeAxis(false, 0, 0, 10, 0),
      z: rangeAxis(false, 0, 0, 10, 0)
    }
  };
}

export function rangeAxis(enabled, start, stop, step, fixed) {
  return {
    enabled,
    mode: "range",
    start,
    stop,
    step,
    fixed,
    list: fixed.toString()
  };
}

export function listAxis(enabled, list, fixed = 0) {
  return {
    enabled,
    mode: "list",
    start: fixed,
    stop: fixed,
    step: 10,
    fixed,
    list
  };
}

export function buildPlan(template, blocks, meta = {}) {
  const plan = structuredClone(template ?? defaultPlanTemplate);
  plan.run_id = sanitizeId(meta.runId ?? plan.run_id ?? "generated_plan");
  plan.operator = nonEmpty(meta.operator, plan.operator ?? "local");
  plan.acquisition_window_ms = toInteger(meta.acquisitionWindowMs ?? plan.acquisition_window_ms ?? 0, "acquisition_window_ms");
  plan.point_settle_ms = toInteger(meta.pointSettleMs ?? plan.point_settle_ms ?? 500, "point_settle_ms");
  delete plan.point_source;
  plan.points = blocks.flatMap((block) => expandBlock(block));
  if (plan.points.length === 0) {
    throw new Error("generated plan has no points");
  }
  return plan;
}

export function expandBlock(block) {
  const prefix = sanitizeId(block.prefix || "block");
  const x = axisValues(block.axes.x, "x");
  const y = axisValues(block.axes.y, "y");
  const z = axisValues(block.axes.z, "z");
  const activeAxes = ["x", "y", "z"].filter((axis) => block.axes[axis].enabled);
  const baseTargets = block.traversal === "bounce_1d_x"
    ? buildBounce1dXTargets(x, y, z, activeAxes)
    : buildRasterTargets(x, y, z);
  const totalPoints = toInteger(block.totalPoints ?? 0, "total_points");
  const targetCount = totalPoints > 0 ? totalPoints : baseTargets.length;
  const points = [];
  for (let index = 0; index < targetCount; index += 1) {
    const target = baseTargets[index % baseTargets.length];
    points.push({
      point_id: `${prefix}_p${(index + 1).toString().padStart(6, "0")}`,
      target_b_nt: target
    });
  }
  return points;
}

export function axisValues(axis, name) {
  if (!axis.enabled) {
    return [toNumber(axis.fixed, `${name}.fixed`)];
  }

  if (axis.mode === "list") {
    const values = parseList(axis.list);
    if (values.length === 0) {
      throw new Error(`${name} list is empty`);
    }
    return values;
  }

  const start = toNumber(axis.start, `${name}.start`);
  const stop = toNumber(axis.stop, `${name}.stop`);
  const step = toNumber(axis.step, `${name}.step`);
  if (step === 0) {
    throw new Error(`${name}.step must not be zero`);
  }
  if (Math.abs(stop - start) < 1e-12) {
    return [round9(start)];
  }
  if (Math.sign(stop - start) !== Math.sign(step)) {
    throw new Error(`${name}.step direction must move from start toward stop`);
  }

  const values = [];
  let value = start;
  let guard = 0;
  while ((step > 0 && value <= stop + 1e-9) || (step < 0 && value >= stop - 1e-9)) {
    values.push(round9(value));
    value += step;
    guard += 1;
    if (guard > 100000) {
      throw new Error(`${name} generated too many values`);
    }
  }
  return values;
}

export function scanKind(block) {
  const count = ["x", "y", "z"].filter((axis) => block.axes[axis].enabled).length;
  return count === 1 ? "1d" : count === 2 ? "2d" : count === 3 ? "3d" : "fixed";
}

export function summarizePlan(plan, blocks) {
  const kinds = [...new Set(blocks.map(scanKind))].join("+");
  return {
    runId: plan.run_id,
    points: plan.points.length,
    scanKind: kinds || "fixed",
    firstPoint: plan.points[0]?.point_id ?? "-",
    lastPoint: plan.points.at(-1)?.point_id ?? "-"
  };
}

function buildRasterTargets(x, y, z) {
  const points = [];
  for (const zv of z) {
    for (const yv of y) {
      for (const xv of x) {
        points.push([xv, yv, zv]);
      }
    }
  }
  return points;
}

function buildBounce1dXTargets(x, y, z, activeAxes) {
  if (activeAxes.length !== 1 || activeAxes[0] !== "x" || y.length !== 1 || z.length !== 1) {
    throw new Error("bounce_1d_x requires only X active with fixed Y and Z");
  }
  const sequence = [...x];
  for (let index = x.length - 2; index >= 1; index -= 1) {
    sequence.push(x[index]);
  }
  return sequence.map((xv) => [xv, y[0], z[0]]);
}

function parseList(value) {
  return String(value ?? "")
    .split(/[,\s;]+/u)
    .map((item) => item.trim())
    .filter(Boolean)
    .map((item) => toNumber(item, "list value"));
}

function sanitizeId(value) {
  const sanitized = String(value ?? "")
    .trim()
    .replace(/[^A-Za-z0-9_-]+/gu, "_")
    .replace(/^_+|_+$/gu, "");
  return sanitized || "generated_plan";
}

function nonEmpty(value, fallback) {
  const trimmed = String(value ?? "").trim();
  return trimmed.length > 0 ? trimmed : fallback;
}

function toNumber(value, name) {
  const number = Number(value);
  if (!Number.isFinite(number)) {
    throw new Error(`${name} must be a finite number`);
  }
  return number;
}

function toInteger(value, name) {
  const number = toNumber(value, name);
  if (!Number.isInteger(number)) {
    throw new Error(`${name} must be an integer`);
  }
  if (number < 0) {
    throw new Error(`${name} must be non-negative`);
  }
  return number;
}

function round9(value) {
  return Math.round(value * 1e9) / 1e9;
}
