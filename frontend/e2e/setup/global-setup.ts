import { spawn } from "node:child_process";
import fs from "node:fs";
import os from "node:os";
import path from "node:path";
import type { FullConfig } from "@playwright/test";

const PID_FILE = path.join("/tmp", "simando-obscura-e2e.pid");
const OBSCURA_PORT = process.env.OBSCURA_PORT || "9222";

async function isObscuraRunning(): Promise<boolean> {
	try {
		const res = await fetch(`http://127.0.0.1:${OBSCURA_PORT}/json/version`);
		return res.ok;
	} catch {
		return false;
	}
}

function ensureSystemFfmpegSymlink() {
	try {
		const ffmpegDir = path.join(
			os.homedir(),
			".cache",
			"ms-playwright",
			"ffmpeg-1011",
		);
		const ffmpegLink = path.join(ffmpegDir, "ffmpeg-linux");
		if (!fs.existsSync(ffmpegLink) && fs.existsSync("/usr/bin/ffmpeg")) {
			fs.mkdirSync(ffmpegDir, { recursive: true });
			fs.symlinkSync("/usr/bin/ffmpeg", ffmpegLink);
		}
	} catch {
		// Ignore if permission denied or already linked
	}
}

export default async function globalSetup(_config: FullConfig) {
	ensureSystemFfmpegSymlink();

	const running = await isObscuraRunning();
	if (running) {
		console.log(
			`[E2E Setup] Connected to existing Obscura CDP server at port ${OBSCURA_PORT}.`,
		);
		return;
	}

	console.log(
		`[E2E Setup] Starting Obscura CDP server on port ${OBSCURA_PORT}...`,
	);
	const proc = spawn(
		"obscura",
		["serve", "--port", OBSCURA_PORT, "--allow-private-network"],
		{
			detached: true,
			stdio: "ignore",
			env: {
				...process.env,
				OBSCURA_ALLOW_PRIVATE_NETWORK: "1",
			},
		},
	);
	proc.unref();

	if (proc.pid) {
		fs.writeFileSync(PID_FILE, proc.pid.toString(), "utf-8");
	}

	// Poll until Obscura CDP is reachable (up to 10 seconds)
	const start = Date.now();
	while (Date.now() - start < 10000) {
		if (await isObscuraRunning()) {
			console.log(
				`[E2E Setup] Obscura CDP server is ready at ws://127.0.0.1:${OBSCURA_PORT}/devtools/browser`,
			);
			return;
		}
		await new Promise((r) => setTimeout(r, 300));
	}

	console.warn(
		`[E2E Setup] Warning: Obscura CDP server did not respond on port ${OBSCURA_PORT} within 10s.`,
	);
}
