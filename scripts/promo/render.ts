import { spawnSync } from "child_process";
import { existsSync } from "fs";
import { join, resolve } from "path";

// Auto-detect Chrome
const candidates = [
  process.env.CHROME_PATH,
  "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe",
  "C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe",
  "C:\\Program Files\\Chromium\\Application\\chrome.exe",
  "/usr/bin/google-chrome",
  "/usr/bin/chromium",
  "/usr/bin/chromium-browser",
  "/snap/bin/chromium",
];

let chromePath: string | undefined;
for (const c of candidates) {
  if (c && existsSync(c)) {
    chromePath = c;
    break;
  }
}

if (!chromePath) {
  console.error("Chrome/Chromium not found. Set CHROME_PATH env var.");
  process.exit(1);
}

const htmlPath = resolve(join(__dirname, "index.html"));
const outputPath = resolve(join(__dirname, "fbox-promo.png"));

console.log(`Rendering ${htmlPath} → ${outputPath}`);
console.log(`Chrome: ${chromePath}`);

const result = spawnSync(
  `"${chromePath}"`,
  [
    "--headless=new",
    `--screenshot="${outputPath}"`,
    "--window-size=1080,1920",
    "--hide-scrollbars",
    "--disable-extensions",
    "--disable-gpu",
    "--no-sandbox",
    "--virtual-time-budget=8000",
    `file:///${htmlPath.replace(/\\/g, "/")}`,
  ],
  { shell: true, stdio: "inherit", timeout: 30000 }
);

if (result.status === 0) {
  console.log(`Done! Output: ${outputPath}`);
} else {
  console.error(`Chrome exited with code ${result.status}`);
  process.exit(1);
}
