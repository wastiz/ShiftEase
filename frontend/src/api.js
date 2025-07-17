import axios from "axios";

const api = axios.create({
    baseURL: import.meta.env.VITE_SERVER_API,
    withCredentials: true
});

api.interceptors.request.use(config => {
    const orgId = localStorage.getItem("orgId");
    const accessToken = localStorage.getItem("accessToken");

    if (orgId) config.headers["X-Organization-Id"] = orgId;
    if (accessToken) config.headers["Authorization"] = `Bearer ${accessToken}`;

    return config;
});

api.interceptors.response.use(
    response => response,
    async (error) => {
        if (error.response && error.response.status === 401) {
            const refreshToken = localStorage.getItem('refreshToken');
            if (!refreshToken) {
                return Promise.reject(error);
            }

            try {
                const newAccessToken = await refreshAccessToken(refreshToken);
                const config = error.config;
                config.headers["Authorization"] = `Bearer ${newAccessToken}`;

                return api(config);
            } catch (refreshError) {
                return Promise.reject(refreshError);
            }
        }

        return Promise.reject(error);
    }
);

api.interceptors.response.use(
    response => response,
    async (error) => {
        const originalRequest = error.config;

        if (originalRequest.url.includes('/identity/refresh')) {
            return Promise.reject(error);
        }

        if (error.response && error.response.status === 401 && !originalRequest._retry) {
            originalRequest._retry = true;

            const refreshToken = localStorage.getItem('refreshToken');
            if (!refreshToken) return Promise.reject(error);

            try {
                const newAccessToken = await refreshAccessToken(refreshToken);
                originalRequest.headers["Authorization"] = `Bearer ${newAccessToken}`;
                return api(originalRequest);
            } catch (refreshError) {
                return Promise.reject(refreshError);
            }
        }

        return Promise.reject(error);
    }
);

async function refreshAccessToken(refreshToken) {
    const response = await fetch('http://localhost:5258/api/identity/refresh', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(refreshToken)
    });

    if (!response.ok) {
        throw new Error('Unable to refresh token');
    }

    const data = await response.json();
    console.log(data)

    localStorage.setItem('accessToken', data.accessToken);
    localStorage.setItem('refreshToken', data.refreshToken);

    return data.accessToken;
}

export default api;
