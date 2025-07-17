import "./EmployeeView.css"
import {ButtonGroup} from "react-bootstrap";
import ToggleButton from "react-bootstrap/ToggleButton";
import Button from "react-bootstrap/Button";
import {useState} from "react";
import EmployeeVacationView from "./EmployeeVacationView.jsx";
import EmployeeScheduleView from "./scheduleView/EmployeeScheduleView.jsx";
import EmployeePreferencesView from "./EmployeePreferencesView.jsx";
import EmployeeSickLeaveView from "./EmployeeSickLeaveView.jsx";
import {useNavigate} from "react-router-dom";
import ToggleGroup from "../assets/ToggleGroup.jsx";
import {LuLogOut} from "react-icons/lu";

function EmployeePersonalPage () {
    const [view, setView] = useState("schedule")
    const navigate = useNavigate();
    const orgId = localStorage.getItem("orgId")

    const handleLogOut = () => {
        localStorage.removeItem("accessToken");
        localStorage.removeItem("refreshToken");
        localStorage.removeItem("employeeId");
        localStorage.removeItem("orgId");
        localStorage.removeItem("groupId");
        navigate("/login");
    }

    return (
        <div className="employee-personal-page">
            <div className="d-flex flex-row align-items-center mb-3">
                <ToggleGroup
                    name="options"
                    value={view}
                    onChange={(val) => {setView(val);}}
                    idPrefix="options"
                    options={[
                        { label: "Schedule", value: "schedule" },
                        { label: "Vacation", value: "vacation" },
                        { label: "Sick Leave", value: "sick_leave" },
                        { label: "Work Preferences", value: "preferences" },
                    ]}
                />
                <LuLogOut onClick={handleLogOut} className="link-text ms-auto" style={{fontSize: "2.5rem"}}/>
            </div>

            {view === "schedule" ? (
                orgId ? <EmployeeScheduleView orgId={orgId}/> : <p>Loading...</p>
            ) : view === "vacation" ? (
                <EmployeeVacationView />
            ) : view === "preferences" ? (
                orgId ? <EmployeePreferencesView orgId={orgId}/> : <p>Loading...</p>
            ) : (
                <EmployeeSickLeaveView />
            )}
        </div>
    )
}

export default EmployeePersonalPage;