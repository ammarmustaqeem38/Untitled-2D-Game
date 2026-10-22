(() => {
  "use strict";

  // Replaced by the exporter with this release's content-addressed asset list.
  const manifest = __WEB_ASSET_MANIFEST__;
  const base = new URL(".", document.currentScript.src);
  const nativeFetch = globalThis.fetch.bind(globalThis);
  const resources = new Map(Object.entries(manifest.files)
    .map(([name, entry]) => [new URL(name, base).href, entry]));

  function assetStream(entry, signal) {
    const abort = new AbortController();
    const onAbort = () => abort.abort(signal.reason);
    if (signal?.aborted) onAbort();
    else signal?.addEventListener("abort", onAbort, { once: true });
    const cleanup = () => signal?.removeEventListener("abort", onAbort);
    const pending = new Map();
    let next = 0;
    let cursor = 0;

    function fill() {
      // Keep a bounded window in flight; large game packs never need to be
      // assembled into a second, full-size JavaScript buffer before loading.
      while (pending.size < 4 && next < entry.parts.length) {
        const part = entry.parts[next];
        const request = nativeFetch(new URL(part.file, base), {
          signal: abort.signal,
          integrity: part.integrity,
        }).then(async (response) => {
          if (!response.ok) throw new Error("The game download failed. Please reload and try again.");
          const bytes = new Uint8Array(await response.arrayBuffer());
          if (bytes.byteLength !== part.size) throw new Error("The game download was incomplete. Please reload and try again.");
          return bytes;
        });
        // Record errors immediately, including for parts not consumed yet.
        pending.set(next++, request.then((bytes) => ({ bytes }), (error) => ({ error })));
      }
    }

    const stream = new ReadableStream({
      start() { fill(); },
      async pull(controller) {
        const result = await pending.get(cursor);
        if (result.error) {
          abort.abort();
          cleanup();
          controller.error(result.error);
          return;
        }
        pending.delete(cursor++);
        controller.enqueue(result.bytes);
        fill();
        if (cursor === entry.parts.length) {
          cleanup();
          controller.close();
        }
      },
      cancel() {
        abort.abort();
        cleanup();
      },
    });

    if (entry.encoding === "gzip") {
      if (typeof DecompressionStream === "undefined") {
        abort.abort();
        cleanup();
        throw new Error("Please update your browser to play this game.");
      }
      return stream.pipeThrough(new DecompressionStream("gzip"));
    }
    return stream;
  }

  // Godot keeps its usual filenames, worker setup, progress UI, and engine API.
  // Only its two large binary downloads use this transport adapter.
  globalThis.fetch = function (input, options) {
    const url = new URL(typeof input === "string" || input instanceof URL ? input : input.url, location.href);
    const method = options?.method || input.method || "GET";
    const entry = resources.get(url.href);
    if (!entry || method.toUpperCase() !== "GET") return nativeFetch(input, options);
    try {
      return Promise.resolve(new Response(assetStream(entry, options?.signal || input.signal), {
        headers: {
          "Content-Type": entry.type,
          "Content-Length": String(entry.size),
        },
      }));
    } catch (error) {
      return Promise.reject(error);
    }
  };
})();
