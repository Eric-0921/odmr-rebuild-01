import {
  buildPlan,
  defaultBlock,
  defaultPlanTemplate,
  rangeAxis,
  summarizePlan
} from "./plan-core.mjs";

let template = structuredClone(defaultPlanTemplate);
let blocks = [defaultBlock("x_line")];
let latestPlan = null;

const elements = {
  runId: document.querySelector("#run-id"),
  operator: document.querySelector("#operator"),
  pointSettle: document.querySelector("#point-settle"),
  acquisitionWindow: document.querySelector("#acquisition-window"),
  templateFile: document.querySelector("#template-file"),
  blocks: document.querySelector("#blocks"),
  addBlock: document.querySelector("#add-block"),
  addXyz: document.querySelector("#add-xyz"),
  generate: document.querySelector("#generate"),
  copyJson: document.querySelector("#copy-json"),
  downloadJson: document.querySelector("#download-json"),
  jsonOutput: document.querySelector("#json-output"),
  summary: document.querySelector("#summary"),
  pointPreview: document.querySelector("#point-preview")
};

renderBlocks();
generatePlan();

elements.addBlock.addEventListener("click", () => {
  syncBlocksFromDom();
  blocks.push(defaultBlock(`block_${blocks.length + 1}`));
  renderBlocks();
});

elements.addXyz.addEventListener("click", () => {
  syncBlocksFromDom();
  blocks.push(singleAxisBlock("x", "x_line"));
  blocks.push(singleAxisBlock("y", "y_line"));
  blocks.push(singleAxisBlock("z", "z_line"));
  renderBlocks();
});

elements.generate.addEventListener("click", generatePlan);

elements.copyJson.addEventListener("click", async () => {
  if (!elements.jsonOutput.value) {
    return;
  }
  if (navigator.clipboard) {
    await navigator.clipboard.writeText(elements.jsonOutput.value);
    return;
  }
  elements.jsonOutput.select();
  document.execCommand("copy");
});

elements.downloadJson.addEventListener("click", () => {
  if (!latestPlan) {
    return;
  }
  const blob = new Blob([JSON.stringify(latestPlan, null, 2) + "\n"], { type: "application/json" });
  const url = URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = `${latestPlan.run_id}.json`;
  link.click();
  URL.revokeObjectURL(url);
});

elements.templateFile.addEventListener("change", async () => {
  const file = elements.templateFile.files?.[0];
  if (!file) {
    return;
  }
  try {
    template = JSON.parse(await file.text());
    elements.runId.value = `${template.run_id ?? "generated_plan"}_generated`;
    elements.operator.value = template.operator ?? "local";
    elements.pointSettle.value = template.point_settle_ms ?? 500;
    elements.acquisitionWindow.value = template.acquisition_window_ms ?? 0;
    generatePlan();
  } catch (error) {
    showError(error);
  }
});

elements.blocks.addEventListener("click", (event) => {
  const removeButton = event.target.closest("[data-remove-block]");
  if (!removeButton) {
    return;
  }
  const index = Number(removeButton.dataset.removeBlock);
  syncBlocksFromDom();
  blocks.splice(index, 1);
  if (blocks.length === 0) {
    blocks.push(defaultBlock("x_line"));
  }
  renderBlocks();
});

function generatePlan() {
  try {
    syncBlocksFromDom();
    latestPlan = buildPlan(template, blocks, {
      runId: elements.runId.value,
      operator: elements.operator.value,
      pointSettleMs: Number(elements.pointSettle.value),
      acquisitionWindowMs: Number(elements.acquisitionWindow.value)
    });
    elements.jsonOutput.value = JSON.stringify(latestPlan, null, 2);
    renderSummary(latestPlan);
    renderPreview(latestPlan);
  } catch (error) {
    latestPlan = null;
    showError(error);
  }
}

function renderBlocks() {
  elements.blocks.innerHTML = blocks.map((block, index) => blockHtml(block, index)).join("");
}

function blockHtml(block, index) {
  return `
    <section class="scan-block" data-block-index="${index}">
      <div class="block-head">
        <div class="field">
          <label>Block prefix</label>
          <input data-field="prefix" value="${escapeHtml(block.prefix)}" />
        </div>
        <div class="field">
          <label>Traversal</label>
          <select data-field="traversal">
            <option value="raster"${selected(block.traversal, "raster")}>raster</option>
            <option value="bounce_1d_x"${selected(block.traversal, "bounce_1d_x")}>bounce_1d_x</option>
          </select>
        </div>
        <div class="field">
          <label>Total points</label>
          <input data-field="totalPoints" type="number" min="0" step="1" value="${block.totalPoints}" />
        </div>
        <button class="danger" type="button" data-remove-block="${index}">Remove</button>
      </div>
      ${axisHtml("x", block.axes.x)}
      ${axisHtml("y", block.axes.y)}
      ${axisHtml("z", block.axes.z)}
    </section>
  `;
}

function axisHtml(axisName, axis) {
  return `
    <div class="axis-row" data-axis="${axisName}">
      <label class="axis-toggle">
        <input data-axis-field="enabled" type="checkbox"${axis.enabled ? " checked" : ""} />
        <span class="axis-name">${axisName.toUpperCase()}</span>
      </label>
      <div class="field">
        <label>Mode</label>
        <select data-axis-field="mode">
          <option value="range"${selected(axis.mode, "range")}>range</option>
          <option value="list"${selected(axis.mode, "list")}>list</option>
        </select>
      </div>
      <div class="field">
        <label>Fixed</label>
        <input data-axis-field="fixed" type="number" step="any" value="${axis.fixed}" />
      </div>
      <div class="field">
        <label>Start</label>
        <input data-axis-field="start" type="number" step="any" value="${axis.start}" />
      </div>
      <div class="field">
        <label>Stop</label>
        <input data-axis-field="stop" type="number" step="any" value="${axis.stop}" />
      </div>
      <div class="field">
        <label>Step</label>
        <input data-axis-field="step" type="number" step="any" value="${axis.step}" />
      </div>
      <div class="field">
        <label>List</label>
        <input data-axis-field="list" value="${escapeHtml(axis.list)}" />
      </div>
    </div>
  `;
}

function syncBlocksFromDom() {
  const renderedBlocks = [...elements.blocks.querySelectorAll("[data-block-index]")];
  if (renderedBlocks.length === 0) {
    return;
  }
  blocks = renderedBlocks.map((blockElement) => {
    const axes = {};
    for (const axisName of ["x", "y", "z"]) {
      const axisElement = blockElement.querySelector(`[data-axis="${axisName}"]`);
      axes[axisName] = {
        enabled: axisElement.querySelector('[data-axis-field="enabled"]').checked,
        mode: axisElement.querySelector('[data-axis-field="mode"]').value,
        fixed: Number(axisElement.querySelector('[data-axis-field="fixed"]').value),
        start: Number(axisElement.querySelector('[data-axis-field="start"]').value),
        stop: Number(axisElement.querySelector('[data-axis-field="stop"]').value),
        step: Number(axisElement.querySelector('[data-axis-field="step"]').value),
        list: axisElement.querySelector('[data-axis-field="list"]').value
      };
    }
    return {
      prefix: blockElement.querySelector('[data-field="prefix"]').value,
      traversal: blockElement.querySelector('[data-field="traversal"]').value,
      totalPoints: Number(blockElement.querySelector('[data-field="totalPoints"]').value),
      axes
    };
  });
}

function singleAxisBlock(axis, prefix) {
  return {
    prefix,
    traversal: axis === "x" ? "bounce_1d_x" : "raster",
    totalPoints: 0,
    axes: {
      x: axis === "x" ? rangeAxis(true, 0, 40, 10, 0) : rangeAxis(false, 0, 0, 10, 0),
      y: axis === "y" ? rangeAxis(true, 0, 40, 10, 0) : rangeAxis(false, 0, 0, 10, 0),
      z: axis === "z" ? rangeAxis(true, 0, 40, 10, 0) : rangeAxis(false, 0, 0, 10, 0)
    }
  };
}

function renderSummary(plan) {
  const summary = summarizePlan(plan, blocks);
  elements.summary.textContent = [
    `run_id: ${summary.runId}`,
    `points: ${summary.points}`,
    `scan: ${summary.scanKind}`,
    `first: ${summary.firstPoint}`,
    `last: ${summary.lastPoint}`
  ].join("\n");
}

function renderPreview(plan) {
  const rows = plan.points.slice(0, 250).map((point) => {
    const [x, y, z] = point.target_b_nt;
    return `
      <tr>
        <td>${escapeHtml(point.point_id)}</td>
        <td>${formatNumber(x)}</td>
        <td>${formatNumber(y)}</td>
        <td>${formatNumber(z)}</td>
      </tr>
    `;
  });
  elements.pointPreview.innerHTML = rows.join("");
}

function showError(error) {
  const message = error instanceof Error ? error.message : String(error);
  elements.summary.textContent = `error: ${message}`;
}

function selected(value, expected) {
  return value === expected ? " selected" : "";
}

function escapeHtml(value) {
  return String(value ?? "")
    .replace(/&/gu, "&amp;")
    .replace(/</gu, "&lt;")
    .replace(/>/gu, "&gt;")
    .replace(/"/gu, "&quot;");
}

function formatNumber(value) {
  return Number(value).toLocaleString("en-US", { maximumFractionDigits: 9, useGrouping: false });
}
