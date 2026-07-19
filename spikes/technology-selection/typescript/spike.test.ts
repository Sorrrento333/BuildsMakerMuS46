import assert from "node:assert/strict";
import { rmSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";
import test from "node:test";
import { calculate, roundTripSqlite } from "./spike.ts";

test("produces the explicit synthetic trace", () => {
  const trace = calculate(10, 3, 4);
  assert.equal(trace.result, 7);
  assert.equal(trace.rounding, "FLOOR");
  assert.equal(trace.fixture, "TECH-SPIKE-ONLY");
});

test("rejects invalid and unsafe arithmetic", () => {
  assert.throws(() => calculate(10, 3, 0), RangeError);
  assert.throws(() => calculate(Number.MAX_SAFE_INTEGER, 2, 1), RangeError);
});

test("round-trips the trace through SQLite", () => {
  const path = join(tmpdir(), `mu-tech-spike-ts-${process.pid}.sqlite`);
  try {
    assert.deepEqual(roundTripSqlite(path, calculate(10, 3, 4)), calculate(10, 3, 4));
  } finally {
    rmSync(path, { force: true });
  }
});
