import './Schedules.css';
import { useState, useEffect } from 'react';
import { useNavigate } from "react-router-dom";
import Cookies from 'js-cookie';
import api from '../../../api.js';
import EntityCheckResult from "../../assets/EntityCheckResult.jsx";
import InfoCard from "../../assets/InfoCard.jsx";
import LoadingSpinner from "../../assets/LoadingSpinner.jsx";


function Schedules() {
    const api_route = import.meta.env.VITE_SERVER_API;
    const token = Cookies.get('auth_token');
    const navigate = useNavigate();
    const [entities, setEntities] = useState({});
    const [schedules, setSchedules] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        const fetchData = async () => {
            try {
                setLoading(true);

                const entitiesResponse = await api.get(`${api_route}/organization/check-entities`, {
                    headers: {
                        'Authorization': `Bearer ${token}`
                    }
                });

                setEntities(entitiesResponse.data);

                if (entitiesResponse.data.groups && entitiesResponse.data.employees && entitiesResponse.data.shiftTypes) {
                    const schedulesResponse = await api.get(
                        `${api_route}/schedule/schedule-summaries`,
                        {
                            headers: {
                                'Authorization': `Bearer ${token}`
                            },
                            validateStatus: (status) => (status >= 200 && status < 300) || status === 404
                        }
                    );

                    if (schedulesResponse.status === 404) {
                        setSchedules([]);
                    } else {
                        setSchedules(schedulesResponse.data);
                    }
                }
            } catch (err) {
                setError(err.response?.data?.message || 'Failed to fetch data');
                console.error('Error:', err);
            } finally {
                setLoading(false);
            }
        };

        fetchData();
    }, [token]);

    const handleEditClick = (groupId) => {
        navigate(`../manage-shifts/${groupId}`)
    };

    if (loading) return <LoadingSpinner visible={loading}/>;
    if (error) return <div className="error">{error}</div>;

    if (!entities.groups || !entities.employees || !entities.shiftTypes) {
        return <EntityCheckResult entities={entities} />;
    }

    return (
        <div className="p-4">
            <div className='d-flex flex-row justify-content-between align-items-center mb-5'>
                <h2>Schedules</h2>
            </div>

            <div className="schedules-container">
                {schedules.map((group) => {
                    const hasSchedules =
                        group.unconfirmedMonths.length > 0 || group.confirmedMonths.length > 0;

                    const text = hasSchedules ? (
                        <>
                            <p>
                                <strong>Shifts confirmed for:</strong>{" "}
                                {group.confirmedMonths.length > 0
                                    ? group.confirmedMonths.join(", ")
                                    : "—"}
                            </p>
                            <p>
                                <strong>Not confirmed for:</strong>{" "}
                                {group.unconfirmedMonths.length > 0
                                    ? group.unconfirmedMonths.join(", ")
                                    : "—"}
                            </p>
                            <p>
                                <strong>Autorenewal:</strong> {group.autorenewal ? "Yes" : "No"}
                            </p>
                        </>
                    ) : (
                        <p>
                            <strong>This group does not have any schedules, please create one</strong>
                        </p>
                    );

                    return (
                        <InfoCard
                            key={group.groupId}
                            title={group.groupName}
                            subtitle={text}
                            actions={[
                                {
                                    label: hasSchedules ? "Manage" : "Create",
                                    variant: "primary",
                                    onClick: () => handleEditClick(group.groupId),
                                },
                            ]}
                            className="schedule-card mb-3"
                        />
                    );
                })}
            </div>

        </div>
    );}

export default Schedules;