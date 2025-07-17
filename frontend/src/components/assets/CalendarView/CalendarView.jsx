import './CalendarView.css';
import DayContainer from "./DayContainer.jsx";
import {useEffect, useState} from "react";


function getDaysInMonth(year, month) {
    const days = [];
    const date = new Date(year, month, 1);

    while (date.getMonth() === month) {
        days.push(date.toLocaleDateString('sv-SE'));
        date.setDate(date.getDate() + 1);
    }

    return days;
}

const today = new Date();
const firstDateStr = today.toISOString().split('T')[0];
const lastDayOfMonth = new Date(today.getFullYear(), today.getMonth() + 1, 0);
const lastDateStr = lastDayOfMonth.toISOString().split('T')[0];
const hours = Array.from({ length: 24 }, (_, i) => `${i.toString().padStart(2, '0')}:00`);

const CalendarView = ({organizationHolidays, workDays, shiftData}) => {

    const [currentMonth, setCurrentMonth] = useState(today.getMonth());
    const [currentYear, setCurrentYear] = useState(today.getFullYear());
    const [daysOfMonth, setDaysOfMonth] = useState(getDaysInMonth(today.getFullYear(), today.getMonth()));
    const [dateFrom, setDateFrom] = useState(firstDateStr);
    const [dateTo, setDateTo] = useState(lastDateStr);

    useEffect(() => {
        const newDays = getDaysInMonth(currentYear, currentMonth);
        setDaysOfMonth(newDays);

        const firstDateStr = `${currentYear}-${String(currentMonth + 1).padStart(2, '0')}-01`;
        const lastDay = newDays.length;
        const lastDateStr = `${currentYear}-${String(currentMonth + 1).padStart(2, '0')}-${String(lastDay).padStart(2, '0')}`;

        setDateFrom(firstDateStr);
        setDateTo(lastDateStr);
    }, [currentMonth, currentYear]);

    return (
        <div className="calendar-grid view-calendar-grid">
            <div className="calendar-column">
                <div className="calendar-cell">Time/Date</div>
                {hours.map((hour) => (
                    <div key={hour} className="calendar-cell">{hour}</div>
                ))}
            </div>

            {daysOfMonth.map((date) => {
                const isHoliday = organizationHolidays.some(holiday =>
                    `2025-${String(holiday.month).padStart(2, "0")}-${String(holiday.day).padStart(2, "0")}` === date
                );

                if (isHoliday) {
                    return (
                        <div key={date} className="calendar-column">
                            <div className="calendar-day-cell calendar-holiday-cell">
                                <div className="calendar-cell">
                                    <strong>{new Date(date).getDate()}</strong>
                                </div>
                            </div>
                        </div>
                    );
                }

                return (
                    <DayContainer
                        key={date}
                        date={date}
                        shiftData={shiftData}
                        workDays={workDays}
                    />
                );
            })}
        </div>
    )
}

export default CalendarView;