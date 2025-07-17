import { useEffect, useState } from 'react';
import ShiftBox from './ShiftBox';

function DayContainer({date, shiftData, workDays}) {

    const [shiftsInDay, setShiftsInDay] = useState([]);

    useEffect(() => {
        const filteredShifts = shiftData.shifts.filter(shift => shift.date === date);
        setShiftsInDay(filteredShifts);
    }, [shiftData, date]);

    const getRowFromTime = (timeStr) => {
        const [h, m] = timeStr.split(":").map(Number);
        return h * 4 + m / 15;
    };

    const weekDayName = new Date(date).toLocaleDateString('en-US', { weekday: 'long' }).toLowerCase();
    const workDay = workDays.find(wd => wd.weekDayName.toLowerCase() === weekDayName);


    return (
        <div className="calendar-column" style={{ position: 'relative' }}>
            <div className="calendar-day-cell">
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
                                backgroundColor: "green",
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
                                backgroundColor: "red",
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
                />
            ))}
        </div>
    );
}

export default DayContainer;
