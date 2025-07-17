import axios from "axios";

const publicApi = axios.create({
    baseURL: import.meta.env.VITE_SERVER_API,
    withCredentials: true,
});

export default publicApi;
