const TOKEN_KEY = "access-token";
const LOGIN_PAGE = "/login";

class HttpService {
    constructor() {
        this.baseUrl = "https://localhost:7205";
    }

    async getAsync(uri) {
        const response = await this.#authFetch(`${this.baseUrl}${uri}`, {
            method: "GET",
            headers: {
                "Content-Type": "application/json",
            },
        });
        if (!response.ok) throw new Error("Error al obtener recursos");
        return await response.json();
    }

    async putAsync(uri, data) {
        const response = await this.#authFetch(`${this.baseUrl}${uri}`, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify(data),
        });
        if (!response.ok) throw new Error("Error al actualizar recursos");
    }

    async #authFetch(url, options = {}) {
        const token = localStorage.getItem(TOKEN_KEY);
        if (!token) {
            window.location.href = LOGIN_PAGE;
            return Promise.reject("No se encontró token");
        }
        options.headers = {
            ...options.headers,
            Authorization: `Bearer ${token}`,
        };
        var response = await fetch(url, options);

        if (response.status === 401) {
            localStorage.removeItem(TOKEN_KEY);
            window.location.href = LOGIN_PAGE;
            return Promise.reject("Token expirado");
        }
        return response;
    }
}

const HTTP_SERVICE = new HttpService();

export default HTTP_SERVICE;
