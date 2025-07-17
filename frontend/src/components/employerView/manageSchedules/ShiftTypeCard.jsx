import { useRef, useEffect } from 'react';
import { draggable } from "@atlaskit/pragmatic-drag-and-drop/element/adapter";

function ShiftTypeCard({ type, id }) {
    const ref = useRef(null);

    useEffect(() => {
        const element = ref.current;

        if (!element) return;

        return draggable({
            element,
            onDragStart() {
                console.log('Started dragging shift type');
            },
            onDrag() {
                console.log('Dragging shift type');
            },
            onDrop({ source }) {
                console.log('Shift type dropped');
            },
            getInitialData() {
                return {
                    id,
                    type: 'shiftType',
                    name: type,
                };
            },
        });
    }, []);

    return (
        <div
            data-draggable-id={`shiftType-${id}`}
            className="draggable-item"
            ref={ref}
        >
            {type}
        </div>
    );
}

export default ShiftTypeCard;