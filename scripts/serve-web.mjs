import { createServer } from "node:http";
import { createReadStream, statSync } from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const root = fileURLToPath(new URL("../docs/", import.meta.url));
const port = Number(process.env.PORT || 8000);
const types = { ".html": "text/html; charset=utf-8", ".js": "text/javascript", ".css": "text/css", ".wasm": "application/wasm", ".pck": "application/octet-stream", ".png": "image/png", ".svg": "image/svg+xml" };

createServer((request, response) => {
  response.setHeader("Cross-Origin-Opener-Policy", "same-origin");
  response.setHeader("Cross-Origin-Embedder-Policy", "require-corp");
  response.setHeader("X-Content-Type-Options", "nosniff");
  response.setHeader("Cache-Control", "no-cache");
  try {
    if (request.method !== "GET" && request.method !== "HEAD") {
      response.writeHead(405, { Allow: "GET, HEAD" }).end();
      return;
    }
    const url = new URL(request.url, "http://localhost");
    let file = path.resolve(root, `.${decodeURIComponent(url.pathname)}`);
    const relative = path.relative(root, file);
    if (relative.startsWith("..") || path.isAbsolute(relative)) {
      response.writeHead(403).end();
      return;
    }
    if (statSync(file).isDirectory()) file = path.join(file, "index.html");
    const stat = statSync(file);
    response.setHeader("Content-Type", types[path.extname(file)] || "application/octet-stream");
    response.setHeader("Content-Length", stat.size);
    if (request.method === "HEAD") response.end();
    else createReadStream(file).pipe(response);
  } catch {
    response.writeHead(404, { "Content-Type": "text/plain" }).end("Not found");
  }
}).listen(port, "127.0.0.1", () => {
  console.log(`Preview with Netlify-compatible game headers: http://localhost:${port}`);
});
