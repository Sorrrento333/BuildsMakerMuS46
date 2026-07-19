import { DatabaseSync } from "node:sqlite";
import { pathToFileURL } from "node:url";

export type Trace = Readonly<{
  fixture: "TECH-SPIKE-ONLY";
  inputs: Readonly<{ value: number; multiplier: number; divisor: number }>;
  operation: "floor((value * multiplier) / divisor)";
  rounding: "FLOOR";
  result: number;
}>;

export function calculate(value: number, multiplier: number, divisor: number): Trace {
  if (!Number.isSafeInteger(value) || !Number.isSafeInteger(multiplier)) {
    throw new RangeError("value and multiplier must be safe integers");
  }
  if (!Number.isSafeInteger(divisor) || divisor <= 0) {
    throw new RangeError("divisor must be a positive safe integer");
  }

  const product = value * multiplier;
  if (!Number.isSafeInteger(product)) {
    throw new RangeError("intermediate product exceeds safe integer range");
  }

  return {
    fixture: "TECH-SPIKE-ONLY",
    inputs: { value, multiplier, divisor },
    operation: "floor((value * multiplier) / divisor)",
    rounding: "FLOOR",
    result: Math.floor(product / divisor),
  };
}

export function roundTripSqlite(databasePath: string, trace: Trace): Trace {
  const database = new DatabaseSync(databasePath);
  try {
    database.exec("CREATE TABLE traces (id INTEGER PRIMARY KEY, payload TEXT NOT NULL)");
    database.prepare("INSERT INTO traces (payload) VALUES (?)").run(JSON.stringify(trace));
    const row = database.prepare("SELECT payload FROM traces WHERE id = 1").get() as { payload: string };
    return JSON.parse(row.payload) as Trace;
  } finally {
    database.close();
  }
}

if (process.argv[1] && import.meta.url === pathToFileURL(process.argv[1]).href) {
  const trace = calculate(10, 3, 4);
  console.log(JSON.stringify(trace));
}
