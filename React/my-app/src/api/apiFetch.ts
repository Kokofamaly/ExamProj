const API_URL = "http://localhost:5071";

export async function apiFetch(
    endpoint: string,
    options: RequestInit = {}
) {
    let accessToken = localStorage.getItem("accessToken");

    let response = await fetch(`${API_URL}${endpoint}`, {
        ...options,
        headers: {
            ...options.headers,
            Authorization: `Bearer ${accessToken}`,
            "Content-Type": "application/json"
        },
        credentials: "include"
    });

    console.log(`initial request. RESPONSE STATUS: ${response.status}`);
    if (response.status === 401) {
        console.log(`UNAUTHORIZED. RESPONSE STATUS: ${response.status}`);
        const refreshResponse = await fetch(`${API_URL}/auth/refresh`, {
            method: "POST",
            credentials: "include"
        });
        console.log(`Refresh request. RESPONSE STATUS: ${refreshResponse.status}`);
        if (!refreshResponse.ok) {
            throw new Error("Session expired");
        }

        const data = await refreshResponse.json();

        accessToken = data.accessToken;
        localStorage.setItem("accessToken", accessToken!);

        response = await fetch(`${API_URL}${endpoint}`, {
            ...options,
            headers: {
                ...options.headers,
                Authorization: `Bearer ${accessToken}`,
                "Content-Type": "application/json"
            },
            credentials: "include"
        });
    }
    console.log(`Returnign response from api fetch. RESPONSE STATUS: ${response.status}`);
    return response;
}