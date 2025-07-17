import { useEffect, useState } from "react";
import "../EmployeeView.css";
import CalendarView from "../../assets/CalendarView/CalendarView.jsx";
import SimpleView from "../../assets/SimpleView/SimpleView.jsx";
import api from "../../../api.js";
import ToggleGroup from "../../assets/ToggleGroup.jsx";
import LoadingSpinner from "../../assets/LoadingSpinner.jsx";
import {toast} from "react-toastify";

const today = new Date();

function EmployeeScheduleView({ orgId }) {
    const api_route = import.meta.env.VITE_SERVER_API;
    const groupId = localStorage.getItem("groupId");

    const [view, setView] = useState("simple");
    const [organizationHolidays, setOrganizationHolidays] = useState([]);
    const [organizationWorkDays, setOrganizationWorkDays] = useState([]);
    const [shiftData, setShiftData] = useState(null);
    const [loading, setLoading] = useState(false);

    const [currentMonth, setCurrentMonth] = useState(today.getMonth());
    const [currentYear, setCurrentYear] = useState(today.getFullYear());

    useEffect(() => {
        const fetchEmployeeSchedule = async () => {
            if (!orgId) return;

            setLoading(true);

            try {
                const response = await api.get(`${api_route}/schedule/get-schedule-for-view`, {
                    params: {
                        groupId: Number(groupId),
                        month: currentMonth + 1,
                        year: currentYear,
                        showOnlyConfirmed: true,
                    }
                });

                console.log(response)

                setOrganizationWorkDays(response.data.workDays);
                setOrganizationHolidays(response.data.holidays);
                setShiftData(response.data.scheduleInfoWithShifts);
                if (response.data.scheduleInfoWithShifts.length === 0) {
                    toast.warning("No schedule found", {className: "toast-warning"});
                }
            } catch (error) {
                console.error("Error fetching organization data:", error);
                toast.error("Failed to fetch schedule", {className: "toast-error"});
            } finally {
                setLoading(false);
            }
        };

        fetchEmployeeSchedule();
    }, [orgId, groupId, currentMonth, currentYear]);

    return (
        <div className="employee-option-view">
            <div className="schedule-container">
                <ToggleGroup
                    name="viewMode"
                    value={view}
                    onChange={(val) => setView(val)}
                    idPrefix="view"
                    className={"mb-3"}
                    options={[
                        { label: "Simple View", value: "simple" },
                        { label: "Calendar View", value: "calendar" },
                    ]}
                />

                <LoadingSpinner visible={loading}/>

                {!loading && !shiftData && <div className="mt-3">No schedule found</div>}

                {!loading && shiftData && view === "simple" && (
                    <SimpleView shiftData={shiftData || []} />
                )}

                {!loading && shiftData && view === "calendar" && (
                    <CalendarView
                        shiftData={shiftData || []}
                        organizationHolidays={organizationHolidays}
                        workDays={organizationWorkDays}
                    />
                )}
            </div>
        </div>
    );
}

export default EmployeeScheduleView;