import "./EmployeeView.css";
import { useEffect, useState } from "react";
import api from "../../api.js";
import { toast } from "react-toastify";
import { useForm } from "react-hook-form";
import Form from "react-bootstrap/Form";
import Button from "react-bootstrap/Button";
import TextInput from "../assets/Inputs/TextInput.jsx";

function EmployeePreferencesView({ orgId }) {
    const api_route = import.meta.env.VITE_SERVER_API;

    const {
        register,
        handleSubmit,
        setValue,
        formState: { errors },
    } = useForm();

    const [shiftTypes, setShiftTypes] = useState([]);
    const weekDays = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"];

    const [selectedShiftTypes, setSelectedShiftTypes] = useState([]);
    const [selectedWeekDays, setSelectedWeekDays] = useState([]);
    const [selectedDayOffDates, setSelectedDayOffDates] = useState([]);
    const [newDayOffDate, setNewDayOffDate] = useState("");
    const [preferences, setPreferences] = useState({});

    useEffect(() => {
        const fetchData = async () => {
            try {
                const shiftResponse = await api.get(`${api_route}/ShiftType/organization`);
                setShiftTypes(shiftResponse.data);

                const preferenceResponse = await api.get(`${api_route}/EmployeeOptions/preferences`);
                const data = preferenceResponse.data;
                setPreferences(data);

                setSelectedShiftTypes(data.shiftTypePreferences || []);
                setSelectedWeekDays(data.weekDayPreferences || []);
                setSelectedDayOffDates(data.dayOffPreferences || []);

            } catch (error) {
                toast.error("Failed to load preference data");
            }
        };

        fetchData();
    }, []);


    console.log(preferences);

    const toggleSelection = (item, list, setList) => {
        setList(prev =>
            prev.includes(item) ? prev.filter(x => x !== item) : [...prev, item]
        );
    };

    const addDayOff = () => {
        if (newDayOffDate && !selectedDayOffDates.includes(newDayOffDate)) {
            setSelectedDayOffDates([...selectedDayOffDates, newDayOffDate]);
            setNewDayOffDate("");
        }
    };

    const removeDayOff = (date) => {
        setSelectedDayOffDates(selectedDayOffDates.filter(d => d !== date));
    };

    const onSubmit = async (data) => {

        const payload = {
            shiftTypePreferences: selectedShiftTypes,
            weekDayPreferences: selectedWeekDays,
            dayOffPreferences: selectedDayOffDates
        };

        console.log(payload);

        try {
            await api.post(`${api_route}/EmployeeOptions/preferences`, payload);
            toast.success("Preferences saved successfully!", {className: "toast-success"});
        } catch (error) {
            toast.error("Failed to save preferences", {className: "toast-error"});
        }
    };

    return (
        <div className="employee-option-view">
            <Form className="preferences-container" onSubmit={handleSubmit(onSubmit)}>
                <div className="form-primary">
                    <h5 className="mb-2">Preferred Shift Types</h5>
                    <div className="d-flex flex-wrap gap-3">
                        {shiftTypes.map(shift => (
                            <Form.Check
                                key={shift.id}
                                type="checkbox"
                                label={shift.name}
                                checked={selectedShiftTypes.includes(shift.id)}
                                onChange={() => toggleSelection(shift.id, selectedShiftTypes, setSelectedShiftTypes)}
                            />
                        ))}
                    </div>
                </div>

                <div className="form-primary">
                    <h5 className="mb-2">Preferred Week Days</h5>
                    <div className="d-flex flex-wrap gap-3">
                        {weekDays.map(day => (
                            <Form.Check
                                key={day}
                                type="checkbox"
                                label={day}
                                checked={selectedWeekDays.includes(day)}
                                onChange={() => toggleSelection(day, selectedWeekDays, setSelectedWeekDays)}
                            />
                        ))}
                    </div>
                </div>

                <div className="form-primary">
                    <h5 className="mb-2">Preferred Day Offs</h5>
                    <div className="d-flex flex-wrap gap-3">
                        <Form.Control
                            type="date"
                            value={newDayOffDate}
                            onChange={(e) => setNewDayOffDate(e.target.value)}
                            className="w-auto"
                        />
                        <Button variant="secondary" type="button" onClick={addDayOff}>
                            Add Day Off
                        </Button>
                    </div>
                    {selectedDayOffDates.length > 0 && (
                        <ul>
                            {selectedDayOffDates.map(date => (
                                <li key={date} className="d-flex align-items-center gap-3">
                                    {date}{" "}
                                    <Button
                                        variant="outline-danger"
                                        type="button"
                                        onClick={() => removeDayOff(date)}
                                    >
                                        X
                                    </Button>
                                </li>
                            ))}
                        </ul>
                    )}
                </div>

                <Button type="submit" variant="success">
                    Save Preferences
                </Button>
            </Form>
        </div>
    );
}

export default EmployeePreferencesView;
