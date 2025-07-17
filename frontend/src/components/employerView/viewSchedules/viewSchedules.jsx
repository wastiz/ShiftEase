import './viewSchedules.css';
import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useParams } from "react-router-dom";
import api from '../../../api.js';
import { FaLongArrowAltRight } from "react-icons/fa";
import SimpleView from '../../assets/SimpleView/SimpleView.jsx';
import CalendarView from '../../assets/CalendarView/CalendarView.jsx';
import Form from 'react-bootstrap/Form';
import ToggleButton from 'react-bootstrap/ToggleButton';
import ToggleButtonGroup from 'react-bootstrap/ToggleButtonGroup';
import Cookies from 'js-cookie';
import LoadingSpinner from "../../assets/LoadingSpinner.jsx";
import ToggleGroup from "../../assets/ToggleGroup.jsx";
import Button from "react-bootstrap/Button";
import SelectInput from "../../assets/Inputs/SelectInput.jsx";

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

function ViewSchedules() {
    const api_route = import.meta.env.VITE_SERVER_API;
    const { orgId } = useParams();
    const token = Cookies.get('auth_token');
    const [view, setView] = useState('simple');
    const [noData, setNoData] = useState(false);
    const [shiftData, setShiftData] = useState(null);
    const [groups, setGroups] = useState([]);
    const [currentGroup, setCurrentGroup] = useState(null);
    const [organizationHolidays, setOrganizationHolidays] = useState([]);
    const [workDays, setWorkDays] = useState([]);
    const [loading, setLoading] = useState(true);
    const navigate = useNavigate();
    //States and variables for dates
    const [currentMonth, setCurrentMonth] = useState(today.getMonth());
    const [currentYear, setCurrentYear] = useState(today.getFullYear());


    useEffect(() => {
        const fetchGroups = async () => {
            try {
                const response = await api.get(`${api_route}/group/by-organization-id`, {
                    headers: {
                        "Authorization": `Bearer ${token}`,
                        "Content-Type": "application/json"
                    }
                });

                if (response.data && response.data.length > 0) {
                    setGroups(response.data);
                    setCurrentGroup(response.data[0]);
                } else {
                    setNoData(true);
                }
            } catch (error) {
                console.error("Error fetching groups:", error);
                setNoData(true);
            }
        };

        fetchGroups();
    }, [orgId, token]);


    useEffect(() => {
        const fetchShiftData = async () => {
            if (!currentGroup) return;

            try {
                setLoading(true);
                const response = await api.get(`${api_route}/schedule/get-schedule-for-view`, {
                    params: {
                        groupId: Number(currentGroup.id),
                        month: currentMonth + 1,
                        year: currentYear,
                        showOnlyConfirmed: false
                    },
                    headers: { 'Authorization': `Bearer ${token}` }
                });

                console.log(response);

                setShiftData(response.data.scheduleInfoWithShifts);
                setOrganizationHolidays(response.data.holidays);
                setWorkDays(response.data.workDays);
                setNoData(false);
            } catch (error) {
                console.error("Error fetching shift data:", error);
                if (error.response?.status === 404 || !error.response) {
                    setNoData(true);
                }
            } finally {
                setLoading(false);
            }
        };

        fetchShiftData();
    }, [currentGroup, currentMonth, currentYear, token]);

    const handleGroupChange = (groupId) => {
        const selectedGroup = groups.find(group => group.id === Number(groupId));
        setCurrentGroup(selectedGroup);
    };

    if (noData || groups.length === 0) {
        return (
            <div className="no-data-message">
                <h4>You don't have any groups to show</h4>
                <p>At first create Group, then add employees, then make shift types and only then create any schedule</p>
                <br />
                <div className='d-flex flex-row align-items-center gap-2'>
                    <Button variant="primary" onClick={() => navigate(`/organization/${orgId}/groups`)}>Create Group</Button>
                    <FaLongArrowAltRight size={28}/>
                    <Button variant="primary" onClick={() => navigate(`/organization/${orgId}/employees`)}>Add Employees</Button>
                    <FaLongArrowAltRight size={28}/>
                    <Button variant="primary" onClick={() => navigate(`/organization/${orgId}/shifts`)}>Create Shift Types</Button>
                    <FaLongArrowAltRight size={28}/>
                    <Button variant="primary" onClick={() => navigate(`/organization/${orgId}/schedules`)}>Create Schedule</Button>
                </div>
            </div>
        );
    }

    if (loading) {
        return <LoadingSpinner visible={loading} />
    }

    return (
        <div className="p-4">
            <header className='shift-table-header'>
                <div className='item'>
                    <Form.Select
                        aria-label="Select group"
                        className='item'
                        value={currentGroup?.id || ''}
                        onChange={(e) => handleGroupChange(e.target.value)}
                    >
                        {groups.map(group => (
                            <option key={group.id} value={group.id}>
                                {group.name}
                            </option>
                        ))}
                    </Form.Select>
                </div>
                <ToggleGroup
                    name="view-toggle"
                    value={view}
                    onChange={(val) => {
                        setView(val === "simple" ? 'simple' : 'calendar');
                    }}
                    options={[
                        {
                            label: "Simple View",
                            value: "simple",
                            onClick: () => setView('simple'),
                        },
                        {
                            label: "Calendar View",
                            value: "calendar",
                            onClick: () => setView('calendar'),
                        },
                    ]}
                    idPrefix="tbg-radio"
                />
                <div className="month-switcher d-flex align-items-center gap-3 item">
                    <Button variant="secondary" onClick={() => {
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

                    <Button variant="secondary" onClick={() => {
                        if (currentMonth === 11) {
                            setCurrentMonth(0);
                            setCurrentYear(prev => prev + 1);
                        } else {
                            setCurrentMonth(prev => prev + 1);
                        }
                    }}>▶
                    </Button>
                </div>
            </header>
            <div className='shift-table-container'>
                {view === 'simple' ? (
                    <SimpleView shiftData={shiftData || []}/>
                ) : (
                    <CalendarView
                        shiftData={shiftData || []}
                        organizationHolidays={organizationHolidays}
                        workDays={workDays}
                    />
                )}
            </div>
        </div>
    );
}

export default ViewSchedules;