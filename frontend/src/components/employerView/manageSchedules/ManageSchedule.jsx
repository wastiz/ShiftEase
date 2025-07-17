import "./Schedules.css";
import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { toast } from 'react-toastify';
import Cookies from 'js-cookie';
import api from '../../../api.js';
import ShiftTypeCard from './ShiftTypeCard.jsx';
import EmployeeCard from './EmployeeCard.jsx';
import DayContainer from './DayContainer.jsx';
import {Badge} from "react-bootstrap";
import SelectInput from "../../assets/Inputs/SelectInput.jsx";
import Form from "react-bootstrap/Form";
import Button from "react-bootstrap/Button";
import LoadingSpinner from "../../assets/LoadingSpinner.jsx";


function getDaysInMonth(year, month) {
    const days = [];
    const date = new Date(year, month, 1);

    while (date.getMonth() === month) {
        days.push(date.toLocaleDateString('sv-SE'));
        date.setDate(date.getDate() + 1);
    }

    return days;
}

const token = Cookies.get('auth_token');
const api_route = import.meta.env.VITE_SERVER_API;

const today = new Date();
const firstDateStr = today.toISOString().split('T')[0];
const lastDayOfMonth = new Date(today.getFullYear(), today.getMonth() + 1, 0);
const lastDateStr = lastDayOfMonth.toISOString().split('T')[0];
const hours = Array.from({ length: 24 }, (_, i) => `${i.toString().padStart(2, '0')}:00`);

const ManageSchedule = () => {
    const { groupId } = useParams();
    const navigate = useNavigate();

    const [organizationHolidays, setOrganizationHolidays] = useState([]);
    const [workDays, setWorkDays] = useState([]);
    const [groups, setGroups] = useState([]);
    const [employees, setEmployees] = useState([]);
    const [shiftTypes, setShiftTypes] = useState([]);
    const [shiftsData, setShiftsData] = useState([]);
    const [error, setError] = useState(false);
    const [loading, setLoading] = useState(false);
    const [selectedGroupId, setSelectedGroupId] = useState(groupId);
    const [currentScheduleId, setCurrentScheduleId] = useState(null);
    const [autorenewal, setAutorenewal] = useState(false);
    const [isConfirmed, setIsConfirmed] = useState(false);
    //States for dates
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


    useEffect(() => {
        const fetchData = async () => {
            try {
                setLoading(true);

                //Base info querying
                const baseResponse = await api.get(`${api_route}/schedule/schedule-data-for-managing`, {
                    headers: { 'Authorization': `Bearer ${token}` }
                });

                const shiftTypesData = baseResponse.data.shiftTypes;

                setOrganizationHolidays(baseResponse.data.organizationHolidays);
                setWorkDays(baseResponse.data.organizationWorkSchedule);
                setGroups(baseResponse.data.groups);
                setEmployees(baseResponse.data.employees);
                setShiftTypes(shiftTypesData);
                setAutorenewal(false); //TODO. Need to set autorenewal by group

                //Schedule Shifts querying
                const scheduleResponse = await api.get(`${api_route}/schedule/schedule-info-with-shifts`, {
                    params: {
                        groupId: Number(groupId),
                        month: currentMonth + 1,
                        year: currentYear,
                        showOnlyConfirmed: false
                    },
                    headers: { 'Authorization': `Bearer ${token}` }
                });

                if (scheduleResponse.data) {
                    const { shifts } = scheduleResponse.data;

                    const updatedShifts = shifts.map(shift => {
                        const shiftType = shiftTypesData.find(st => st.id === shift.shiftTypeId);

                        if (!shiftType) {
                            console.warn(`ShiftType с id=${shift.shiftTypeId} не найден`);
                            return shift;
                        }

                        return {
                            ...shift,
                            shiftType: shiftType.name,
                            startTime: shiftType.startTime,
                            endTime: shiftType.endTime,
                            color: shiftType.color,
                            employeeNeeded: shiftType.employeeNeeded
                        };
                    });

                    setShiftsData(updatedShifts);
                    setIsConfirmed(scheduleResponse.data.scheduleInfo.isConfirmed);
                    setCurrentScheduleId(scheduleResponse.data.scheduleInfo.id);
                }

                setSelectedGroupId(Number(groupId));
            } catch (err) {
                setError('Failed to load schedule data');
                toast.error('Failed to load schedule data', {className: "toast-error"});
                console.error(err);
            } finally {
                setLoading(false);
            }
        };

        fetchData();
    }, [groupId, currentMonth, currentYear]);

    const transformShiftsData = () => {
        return shiftsData.map(shift => {
            const shiftType = shiftTypes.find(st => st.name === shift.shiftType);
            return {
                shiftTypeId: shiftType?.id || 0,
                date: shift.date,
                employeeIds: shift.employees.map(emp => emp.id)
            };
        }).filter(shift => shift.shiftTypeId !== 0);
    };

    const handleSubmit = async (isConfirmed) => {
        try {
            const transformedShifts = transformShiftsData();
            if (transformedShifts.length === 0) {
                toast.error('No valid shiftTypes to save', {className: "toast-error"});
                return;
            }

            const payload = {
                groupId: Number(selectedGroupId),
                dateFrom,
                dateTo,
                autorenewal,
                isConfirmed,
                shifts: transformedShifts
            };

            await api.post(`${api_route}/schedule/update-schedule`, payload, {
                headers: { 'Authorization': `Bearer ${token}` }
            });

            if (isConfirmed) {
                setIsConfirmed(true);
            }

            toast.success('Schedule saved!', {className: "toast-success"});
        } catch (error) {
            toast.error('Failed to save schedule', {className: "toast-error"});
            console.error(error);
        }
    };

    const handleUnconfirmSchedule = async (scheduleId) => {
        await api.post(`${api_route}/schedule/unconfirm/${scheduleId}`, null, {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        setIsConfirmed(false);
        toast.success('Schedule unconfirmed!');
    }


    if (loading) return <LoadingSpinner visible={loading} />;
    if (error) return <div className="error">{error}</div>;

    return (
        <div className="schedule-page">
            <div className="schedule-sidebar">
                <div className="sidebar-section">
                    <h4>Shift Types</h4>
                    {shiftTypes.map(shift => (
                        <ShiftTypeCard key={shift.id} type={shift.name} id={shift.id} />
                    ))}
                </div>

                <div className="sidebar-section">
                    <h4>Employees</h4>
                    {employees.map((employee) => (
                        <EmployeeCard key={employee.id} name={employee.name} id={employee.id} />
                    ))}
                </div>
            </div>

            <div className="schedule-main">
                <div className="schedule-controls">
                    <div className="d-flex flex-row align-items-center gap-3">
                        <Form.Select
                            className="w-auto"
                            value={selectedGroupId}
                            onChange={(e) => setSelectedGroupId(Number(e.target.value))}
                        >
                            {groups.map(group => (
                                <option value={group.id} key={group.id}>{group.name}</option>
                            ))}
                        </Form.Select>

                        <div className="month-switcher d-flex align-items-center gap-3">
                            <Button variant="success" onClick={() => {
                                if (currentMonth === 0) {
                                    setCurrentMonth(11);
                                    setCurrentYear(prev => prev - 1);
                                } else {
                                    setCurrentMonth(prev => prev - 1);
                                }
                            }}>◀
                            </Button>

                            <h5>{new Date(currentYear, currentMonth).toLocaleString('default', {
                                month: 'long',
                                year: 'numeric'
                            })}</h5>

                            <Button variant="success" onClick={() => {
                                if (currentMonth === 11) {
                                    setCurrentMonth(0);
                                    setCurrentYear(prev => prev + 1);
                                } else {
                                    setCurrentMonth(prev => prev + 1);
                                }
                            }}>▶
                            </Button>
                        </div>

                        <div className="form-group form-check">
                            <input
                                type="checkbox"
                                id="autorenewal"
                                checked={autorenewal}
                                onChange={(e) => setAutorenewal(e.target.checked)}
                                className="form-check-input mb-0"
                            />
                            <label htmlFor="autorenewal" className="form-check-label">Autorenewal</label>
                        </div>

                        <Button variant="success" onClick={() => handleSubmit(false)}>Save</Button>

                        <Button variant="success" onClick={()=> handleSubmit(true)} >Save & Confirm</Button>

                        {isConfirmed ? (
                            <div className={"d-flex flex-column gap-1"}>
                                <Badge bg="info">Confirmed</Badge>
                                <button className={"btn-unconfirm"} onClick={() => handleUnconfirmSchedule(currentScheduleId)}>Unconfirm</button>
                            </div>
                        ) : null}
                    </div>
                </div>

                <div className="calendar-wrapper">
                    <div className="calendar-grid manage-calendar-grid">
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
                                    shiftTypes={shiftTypes}
                                    shiftsData={shiftsData}
                                    setShiftsData={setShiftsData}
                                    employees={employees}
                                    setEmployees={setEmployees}
                                    workDays={workDays}
                                />
                            );
                        })}
                    </div>
                </div>
            </div>
        </div>
    );
};

export default ManageSchedule;
