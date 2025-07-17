import { useState } from "react";
import { Form, Button } from "react-bootstrap";
import { toast } from "react-toastify";

const monthNames = [
    "January", "February", "March", "April", "May", "June",
    "July", "August", "September", "October", "November", "December"
];

const defaultHolidays = [
    { holidayName: "New Year's Day", month: 1, day: 1 },
    { holidayName: "International Workers' Day", month: 5, day: 1 },
    { holidayName: "International Women's Day", month: 3, day: 8 },
    { holidayName: "Christmas Day", month: 12, day: 25 }
];

const isSameHoliday = (a, b) =>
    a.holidayName === b.holidayName &&
    a.day === b.day &&
    a.month === b.month;

const HolidaySelector = ({ selectedHolidays, setSelectedHolidays }) => {
    const [holidays, setHolidays] = useState(defaultHolidays);
    const [newHolidayName, setNewHolidayName] = useState("");
    const [newHolidayDate, setNewHolidayDate] = useState("");

    const handleToggle = (holiday, checked) => {
        setSelectedHolidays(prev =>
            checked
                ? [...prev, holiday]
                : prev.filter(h => !isSameHoliday(h, holiday))
        );
    };

    const handleAddHoliday = () => {
        const name = newHolidayName.trim();
        if (!name || !newHolidayDate) {
            toast.error("Please enter both name and date", {className: "toast-error"});
            return;
        }

        const [year, month, day] = newHolidayDate.split("-").map(Number);
        const newEntry = { holidayName: name, month, day };

        if (holidays.some(h => isSameHoliday(h, newEntry))) {
            toast.warning("This holiday already exists", {className: "toast-warning"});
            return;
        }

        setHolidays(prev => [...prev, newEntry]);
        setSelectedHolidays(prev => [...prev, newEntry]);
        setNewHolidayName("");
        setNewHolidayDate("");
        toast.success("Holiday added", {className: "toast-success"});
    };

    return (
        <>
            <div className="input-card">
                <Form.Label column="sm">Select Holidays</Form.Label>

                <div className="days-selector mt-3">
                    {holidays.map((holiday, idx) => {
                        const isChecked = selectedHolidays.some(h => isSameHoliday(h, holiday));
                        const label = `${holiday.holidayName} (${holiday.day} ${monthNames[holiday.month - 1]})`;

                        return (
                            <Form.Check
                                key={`${holiday.holidayName}-${idx}`}
                                type="checkbox"
                                label={label}
                                checked={isChecked}
                                onChange={(e) => handleToggle(holiday, e.target.checked)}
                                className="mb-2"
                            />
                        );
                    })}
                </div>


            </div>

            <div className="d-flex flex-wrap gap-3 mt-3">
                <Form.Control
                    type="text"
                    placeholder="Holiday name"
                    value={newHolidayName}
                    onChange={(e) => setNewHolidayName(e.target.value)}
                    className="w-auto"
                />
                <Form.Control
                    type="date"
                    value={newHolidayDate}
                    onChange={(e) => setNewHolidayDate(e.target.value)}
                    className="w-auto"
                />
                <Button variant="secondary" type="button" className="ms-auto" onClick={handleAddHoliday}>
                    Add Holiday
                </Button>
            </div>
        </>

    );
};

export default HolidaySelector;
