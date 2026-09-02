import fs from "node:fs";
import path from "node:path";
import type { FullConfig } from "@playwright/test";

const PID_FILE = path.join("/tmp", "simando-obscura-e2e.pid");

export default async function globalTeardown(_config: FullConfig) {
	if (fs.existsSync(PID_FILE)) {
		try {
			const pidStr = fs.readFileSync(PID_FILE, "utf-8").trim();
			const pid = Number.parseInt(pidStr, 10);
			if (!Number.isNaN(pid)) {
				process.kill(pid, "SIGTERM");
				console.log(`[E2E Teardown] Stopped spawned Obscura server (PID ${pid}).`);
			}
		} catch {
			// Process may already have stopped
		} finally {
			try {
				fs.unlinkSync(PID_FILE);
			} catch {}
		}
	}
}
