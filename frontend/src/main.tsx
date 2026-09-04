// Defensive safeguard for synthetic/headless browser DOM reconciliation desyncs
if (typeof Node !== "undefined") {
	const origNodeRemoveChild = Node.prototype.removeChild;
	Node.prototype.removeChild = function <T extends Node>(child: T): T {
		if (!child || child.parentNode !== this) {
			if (child?.parentNode) {
				return child.parentNode.removeChild(child) as T;
			}
			return child;
		}
		return origNodeRemoveChild.call(this, child) as T;
	};
}
if (typeof DocumentFragment !== "undefined") {
	const origFragmentRemoveChild = DocumentFragment.prototype.removeChild;
	DocumentFragment.prototype.removeChild = function <T extends Node>(
		child: T,
	): T {
		if (!child || child.parentNode !== this) {
			if (child?.parentNode) {
				return child.parentNode.removeChild(child) as T;
			}
			return child;
		}
		return origFragmentRemoveChild.call(this, child) as T;
	};
}

import { RouterProvider } from "@tanstack/react-router";
import ReactDOM from "react-dom/client";
import { getRouter } from "./router";

const router = getRouter();

const rootElement = document.getElementById("app");

if (rootElement) {
	const root = ReactDOM.createRoot(rootElement);
	root.render(<RouterProvider router={router} />);
}
