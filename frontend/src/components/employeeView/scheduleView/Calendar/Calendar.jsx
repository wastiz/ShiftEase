import {useEffect, useRef} from "react";
import "./Calendar.css";

const Calendar = ({ schedule }) => {
    const shifts = schedule?.shifts ?? [];

    const fixedColumnRef = useRef(null);
    const scrollableRef = useRef(null);

    const generateDateRange = (start, end) => {
        const dateArray = [];
        const current = new Date(start);
        const endDate = new Date(end);

        while (current <= endDate) {
            const isoDate = current.toISOString().split("T")[0];
            dateArray.push(isoDate);
            current.setDate(current.getDate() + 1);
        }

        return dateArray;
    };

    const uniqueDates = generateDateRange(schedule.startDate, schedule.endDate);

    const uniqueEmployees = [...new Set(
        shifts.flatMap(shift => shift.employees.map(e => e.name))
    )];

    useEffect(() => {
        const fixed = fixedColumnRef.current;
        const scrollable = scrollableRef.current;

        if (!fixed || !scrollable) return;

        const onScroll = () => {
            fixed.scrollTop = scrollable.scrollTop;
        };
        scrollable.addEventListener("scroll", onScroll);

        return () => scrollable.removeEventListener("scroll", onScroll);
    }, [schedule]);

    return (
        <div className="calendar-wrapper">
            <div className="calendar-fixed" ref={fixedColumnRef}>
                <table>
                    <thead>
                    <tr>
                        <th>Employee</th>
                    </tr>
                    </thead>
                    <tbody>
                    {uniqueEmployees.map(name => (
                        <tr key={name}>
                            <td>{name}</td>
                        </tr>
                    ))}
                    </tbody>
                </table>
            </div>

            <div className="calendar-scrollable" ref={scrollableRef}>
                <table>
                    <thead>
                    <tr>
                        {uniqueDates.map(date => (
                            <th key={date}>{date}</th>
                        ))}
                    </tr>
                    </thead>
                    <tbody>
                    {uniqueEmployees.map(name => (
                        <tr key={name}>
                            {uniqueDates.map(date => {
                                const shift = shifts.find(s =>
                                    s.date === date &&
                                    s.employees.some(e => e.name === name)
                                );

                                return (
                                    <td key={date}>
                                        {shift ? `${shift.startTime} - ${shift.endTime}` : ""}
                                    </td>
                                );
                            })}
                        </tr>
                    ))}
                    </tbody>
                </table>
            </div>
        </div>
    );
};

export default Calendar;
