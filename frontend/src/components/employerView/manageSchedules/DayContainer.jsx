import { useRef, useEffect, useState } from 'react';
import { dropTargetForElements } from "@atlaskit/pragmatic-drag-and-drop/element/adapter";
import { toast } from "react-toastify";
import ShiftBox from './ShiftBox.jsx';

function DayContainer({
                          date,
                          shiftsData,
                          setShiftsData,
                          shiftTypes,
                          employees,
                          setEmployees,
                          workDays
                      }) {
    const ref = useRef(null);
    const [shiftsInDay, setShiftsInDay] = useState([]);
    const [isDraggingOver, setIsDraggingOver] = useState(false);

    useEffect(() => {
        const filteredShifts = shiftsData.filter(shift => shift.date === date);
        setShiftsInDay(filteredShifts);
    }, [shiftsData, date]);

    useEffect(() => {
        const element = ref.current;
        if (!element) return;

        return dropTargetForElements({
            element,
            onDrop: ({ source }) => {
                if (source.data.type === "shiftType") {
                    const shiftType = shiftTypes.find(st => st.id === source.data.id);

                    if (shiftType) {
                        const alreadyExists = shiftsData.some(shift =>
                            shift.date === date && shift.shiftType === shiftType.name
                        );

                        if (alreadyExists) {
                            toast.warning("This shift type is already assigned for this day");
                            setIsDraggingOver(false);
                            return;
                        }

                        const newShift = {
                            id: Date.now(),
                            shiftType: shiftType.name,
                            startTime: shiftType.startTime,
                            endTime: shiftType.endTime,
                            date: date,
                            color: shiftType.color,
                            employees: [],
                        };

                        setShiftsData(prev => [...prev, newShift]);
                        setIsDraggingOver(false);
                    }
                }
            },
            onDragEnter: ({ source }) => {
                if (source.data.type === "shiftType") {
                    setIsDraggingOver(true);
                }
            },
            onDragLeave: ({ source }) => {
                if (source.data.type === "shiftType") {
                    setIsDraggingOver(false);
                }
            }
        });
    }, [date, shiftTypes, shiftsData, setShiftsData]);

    const onShiftBoxDelete = (shiftId) => {
        setShiftsData(prev => prev.filter(shift => shift.id !== shiftId));
    };

    const onEmployeeDelete = (shiftId, employeeId) => {
        setShiftsData(prev =>
            prev.map(shift =>
                shift.id === shiftId
                    ? {
                        ...shift,
                        employees: shift.employees.filter(emp => emp.id !== employeeId)
                    }
                    : shift
            )
        );
    };

    const getRowFromTime = (timeStr) => {
        const [h, m] = timeStr.split(":").map(Number);
        return h * 4 + m / 15;
    };

    const weekDayName = new Date(date).toLocaleDateString('en-US', { weekday: 'long' }).toLowerCase();
    const workDay = workDays.find(wd => wd.weekDayName.toLowerCase() === weekDayName);

    return (
        <div ref={ref} className="calendar-column" style={{ position: 'relative' }}>
            <div className={`calendar-day-cell ${isDraggingOver ? "drag-over" : ""}`}>
                <div className={"calendar-cell"}>
                    <strong>
                        {`${String(new Date(date).getDate()).padStart(2, "0")}.${String(new Date(date).getMonth() + 1).padStart(2, "0")}`}
                    </strong>
                </div>
            </div>

            {(() => {
                if (!workDay) {
                    return (
                        <div
                            className="non-working-hours"
                            style={{
                                gridRow: `1 / -1`,
                                background: "var(--bg-tertiary)",
                                zIndex: 0,
                                position: "absolute",
                                top: "60px",
                                width: "100%",
                                height: "96%",
                                pointerEvents: "none",
                            }}
                        />
                    );
                }

                const startRow = getRowFromTime(workDay.from);
                const endRow = getRowFromTime(workDay.to);

                return (
                    <>
                        <div
                            className="workday-line"
                            style={{
                                position: "absolute",
                                top: `${(startRow + 4) * 15}px`,
                                left: 0,
                                width: "100%",
                                height: "1px",
                                backgroundColor: "var(--primary)",
                                zIndex: 1
                            }}
                        />
                        <div
                            className="workday-line"
                            style={{
                                position: "absolute",
                                top: `${(endRow + 4) * 15}px`,
                                left: 0,
                                width: "100%",
                                height: "1px",
                                backgroundColor: "var(--bg-danger)",
                                zIndex: 1
                            }}
                        />
                    </>
                );
            })()}

            {shiftsInDay.map(shift => (
                <ShiftBox
                    key={shift.id}
                    shift={shift}
                    date={date}
                    employees={employees}
                    shiftsData={shiftsData}
                    shiftTypes={shiftTypes}
                    setEmployees={setEmployees}
                    setShiftsData={setShiftsData}
                    onShiftBoxDelete={onShiftBoxDelete}
                    onEmployeeDelete={onEmployeeDelete}
                />
            ))}
        </div>
    );
}

export default DayContainer;
