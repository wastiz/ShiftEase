import {useEffect, useRef, useState} from 'react';
import { dropTargetForElements } from "@atlaskit/pragmatic-drag-and-drop/element/adapter";
import {toast} from "react-toastify";

function ShiftBox({ shift, date, employees, shiftsData, shiftTypes, setEmployees, setShiftsData, onShiftBoxDelete, onEmployeeDelete }) {
    const ref = useRef(null);
    const [isDraggingOver, setIsDraggingOver] = useState(false);

    useEffect(() => {
        const element = ref.current;
        if (!element) return;

        return dropTargetForElements({
            element,
            onDrop: ({ source }) => {
                if (source.data.type === "employee") {
                    const employee = employees.find(emp => emp.id === source.data.id);

                    if (employee) {
                        const alreadyAssigned = shiftsData.some(shift => {
                            return shift.date === date && shift.employees.some(emp => emp.id === employee.id);
                        });


                        if (alreadyAssigned) {
                            toast.warning("This employee is already assigned to this shift");
                            setIsDraggingOver(false);
                            return;
                        }

                        setShiftsData(prev =>
                            prev.map(s =>
                                s.id === shift.id
                                    ? { ...s, employees: [...s.employees, employee] }
                                    : s
                            )
                        );

                        setIsDraggingOver(false);
                    }
                }
            },
            onDragEnter: ({ source }) => {
                if (source.data.type === "employee") {
                    setIsDraggingOver(true);
                }
            },
            onDragLeave: ({ source }) => {
                if (source.data.type === "employee") {
                    setIsDraggingOver(false);
                }
            }
        });
    }, [employees, shift.id, shiftsData, setShiftsData, setEmployees]);


    const getStartRow = (startTime) => {
        const hours = parseInt(startTime.split(":")[0]);
        return hours + 2;
    };

    const getEndRow = (endTime) => {
        const hours = parseInt(endTime.split(":")[0]);
        if (hours === 0) {
            return 26;
        }
        return hours + 2;
    };

    return (
        <div
            ref={ref}
            className={`shift-box ${isDraggingOver ? "drag-over" : ""}`}
            style={{
                gridRow: `${getStartRow(shift.startTime)} / ${getEndRow(shift.endTime)}`,
            }}
        >
            <div className="shift-header">
                {shift.startTime.slice(0, -3)} - {shift.endTime.slice(0, -3)}
                <button
                    onClick={() => onShiftBoxDelete(shift.id)}
                    className="delete-button"
                >
                    &times;
                </button>
            </div>

            <div className="shift-type">{shift.shiftType}</div>

            <div className="shift-avatars">
                {shift.employees.slice(0, 3).map((emp) => (
                    <div
                        key={emp.id}
                        className="avatar-circle hover-removable"
                        title={`Click to remove ${emp.name}`}
                        onClick={() => onEmployeeDelete(shift.id, emp.id)}
                    >
                        <span className="avatar-initial">{emp.name[0]}</span>
                        <span className="avatar-remove">×</span>
                    </div>
                ))}

                {shift.employees.length > 3 && (
                    <div className="avatar-circle extra">+{shift.employees.length - 3}</div>
                )}
            </div>

        </div>
    );
}

export default ShiftBox;