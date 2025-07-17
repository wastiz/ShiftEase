import { useRef, useEffect } from 'react';
import { draggable } from "@atlaskit/pragmatic-drag-and-drop/element/adapter";

function EmployeeCard({ id, name }) {
    const ref = useRef(null);

    useEffect(() => {
        const element = ref.current;

        if (!element) return;

        return draggable({
            element,
            onDragStart() {
                console.log('Started dragging employee');
            },
            onDrag() {
                console.log('Dragging employee');
            },
            onDrop({ source }) {
                console.log('Employee dropped');
            },
            getInitialData() {
                return {
                    id,
                    type: 'employee',
                    name,
                };
            },
        });
    }, []);

    return (
        <div
            data-draggable-id={`employee-${id}`}
            className="draggable-item"
            ref={ref}
        >
            {name}
        </div>
    );
}

export default EmployeeCard;