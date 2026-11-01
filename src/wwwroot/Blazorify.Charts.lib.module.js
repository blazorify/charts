// Blazorify.Charts JavaScript initializer, loaded automatically by Blazor. Rendering never depends on it.
//
// It holds small generic primitives that components reuse, never feature-specific scripts.
//
// play: SVG animation elements (<animate>, <animateTransform>, <animateMotion>, <set>) marked with
// data-bc-play are started when Blazor inserts them. A newly inserted <svg> starts its own timeline, so
// begin="0s" works there without help, but an element added to an svg that is already on the page joins a
// timeline that has long started, and would jump straight to its end. Components give such elements
// begin="indefinite", data-bc-play and a @key per data version, so each update is a fresh insert played here.

const playSelector = "[data-bc-play]";

function play(node) {
	if (node.nodeType !== Node.ELEMENT_NODE) {
		return;
	}

	if (node.matches(playSelector)) {
		node.beginElement?.();
	}

	for (const element of node.querySelectorAll(playSelector)) {
		element.beginElement?.();
	}
}

let started = false;

function start() {
	if (started) {
		return;
	}

	started = true;

	new MutationObserver(mutations => {
		for (const mutation of mutations) {
			for (const node of mutation.addedNodes) {
				play(node);
			}
		}
	}).observe(document, { childList: true, subtree: true });
}

// Blazor Web App (blazor.web.js)
export function afterWebStarted() {
	start();
}

// Standalone WebAssembly, Server (blazor.server.js) and Hybrid
export function afterStarted() {
	start();
}
