import createFetchClient from "openapi-fetch";
import createClient from "openapi-react-query";
import type { paths } from "./schema";

const customFetch: typeof fetch = async (input, init) => {
	if (input instanceof Request) {
		const url = input.url;
		const method = input.method;
		const headers: Record<string, string> = {};
		input.headers.forEach((val, key) => {
			headers[key] = val;
		});
		let body: BodyInit | null | undefined = init?.body;
		if (body === undefined && method !== "GET" && method !== "HEAD") {
			try {
				body = await input.clone().text();
			} catch {
				// body extraction fallback
			}
		}
		return fetch(url, {
			...init,
			method,
			headers: {
				...headers,
				...((init?.headers as Record<string, string>) || {}),
			},
			body: body || undefined,
		});
	}
	return fetch(input, init);
};

export const fetchClient = createFetchClient<paths>({
	baseUrl: "",
	fetch: customFetch,
});

export const $api = createClient(fetchClient);
