import assert from "node:assert/strict";
import {
  buildPlan,
  defaultBlock,
  defaultPlanTemplate,
  expandBlock,
  listAxis,
  rangeAxis
} from "../src/plan-core.mjs";

const oneDimBounce = defaultBlock("x_bounce");
oneDimBounce.traversal = "bounce_1d_x";
oneDimBounce.totalPoints = 21;
oneDimBounce.axes.x = rangeAxis(true, 0, 40, 10, 0);
oneDimBounce.axes.y = rangeAxis(false, 0, 0, 10, 0);
oneDimBounce.axes.z = rangeAxis(false, 0, 0, 10, 0);
const oneDimPoints = expandBlock(oneDimBounce);
assert.equal(oneDimPoints.length, 21);
assert.deepEqual(oneDimPoints.slice(0, 8).map((point) => point.target_b_nt[0]), [0, 10, 20, 30, 40, 30, 20, 10]);
assert.deepEqual(oneDimPoints.at(-1).target_b_nt, [40, 0, 0]);

const twoDimRaster = defaultBlock("xy_plane");
twoDimRaster.axes.x = listAxis(true, "0, 10");
twoDimRaster.axes.y = listAxis(true, "0, 20");
twoDimRaster.axes.z = rangeAxis(false, 0, 0, 10, 0);
const twoDimPoints = expandBlock(twoDimRaster);
assert.deepEqual(twoDimPoints.map((point) => point.target_b_nt), [
  [0, 0, 0],
  [10, 0, 0],
  [0, 20, 0],
  [10, 20, 0]
]);

const threeDimRaster = defaultBlock("xyz_volume");
threeDimRaster.axes.x = listAxis(true, "0 10");
threeDimRaster.axes.y = listAxis(true, "0 20");
threeDimRaster.axes.z = listAxis(true, "0 30");
assert.equal(expandBlock(threeDimRaster).length, 8);

const plan = buildPlan(defaultPlanTemplate, [oneDimBounce, twoDimRaster, threeDimRaster], {
  runId: "fixture xyz blocks",
  operator: "local",
  pointSettleMs: 500,
  acquisitionWindowMs: 0
});
assert.equal(plan.run_id, "fixture_xyz_blocks");
assert.equal(plan.points.length, 33);
assert.equal(plan.point_source, undefined);
assert.equal(plan.points[0].point_id, "x_bounce_p000001");
assert.equal(plan.points.at(-1).point_id, "xyz_volume_p000008");

assert.throws(() => expandBlock({
  prefix: "bad",
  traversal: "bounce_1d_x",
  totalPoints: 0,
  axes: {
    x: rangeAxis(true, 0, 10, 10, 0),
    y: rangeAxis(true, 0, 10, 10, 0),
    z: rangeAxis(false, 0, 0, 10, 0)
  }
}), /requires only X active/u);
