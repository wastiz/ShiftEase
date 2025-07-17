import { useEffect, useState } from "react";
import Spinner from "react-bootstrap/Spinner";

function LoadingSpinner({ visible, delay = 300, minDuration = 1000 }) {
    const [shouldRender, setShouldRender] = useState(false);
    const [renderStart, setRenderStart] = useState(null);

    useEffect(() => {
        let delayTimer;
        let minTimer;

        if (visible) {
            delayTimer = setTimeout(() => {
                setShouldRender(true);
                setRenderStart(Date.now());
            }, delay);
        } else if (shouldRender) {
            const elapsed = Date.now() - renderStart;
            const remaining = Math.max(minDuration - elapsed, 0);
            minTimer = setTimeout(() => setShouldRender(false), remaining);
        }

        return () => {
            clearTimeout(delayTimer);
            clearTimeout(minTimer);
        };
    }, [visible]);

    if (!shouldRender) return null;

    return (
        <div className="h-75 d-flex flex-column justify-content-center align-items-center gap-2 mb-3 mt-3">
            <Spinner animation="border" role="status" />
            <p>Loading</p>
        </div>
    );
}

export default LoadingSpinner;
