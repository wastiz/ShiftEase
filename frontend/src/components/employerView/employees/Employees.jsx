import './Employees.css';
import { useState, useEffect } from 'react';
import EmployeeAddForm from './EmployeeAddForm.jsx';
import AsideModal from '../../assets/asideModal/AsideModal.jsx';
import {FaExpand} from 'react-icons/fa';
import { toast } from 'react-toastify';
import api from '../../../api.js';
import {useNavigate} from "react-router-dom";
import Button from "react-bootstrap/Button";
import InfoCard from "../../assets/InfoCard.jsx";

const Employees = () => {
    const orgId = localStorage.getItem('orgId');
    const [noData, setNoData] = useState(false);
    const navigate = useNavigate();
    const [modalOn, setModalOn] = useState(false);
    const [selectedEmployeeId, setSelectedEmployeeId] = useState(null);
    const [selectedEmployeeData, setSelectedEmployeeData] = useState(null);
    const [expandedEmployeeId, setExpandedEmployeeId] = useState(null);
    const [employees, setEmployees] = useState([]);
    const [groups, setGroups] = useState([]);
    //Modal
    const [show, setShow] = useState(false);
    const handleModalClose = () => setShow(false);
    const handleModalShow = () => setShow(true);

    useEffect(() => {
        const fetchData = async () => {
            try {
                const [employeeResponse, groupResponse] = await Promise.allSettled([
                    api.get(`/employee/by-organization-id/${orgId}`),
                    api.get(`/group/by-organization-id`)
                ]);

                if (groupResponse.status === "fulfilled") {
                    if (groupResponse.value.data.length > 0) {
                        setGroups(groupResponse.value.data);
                    } else {
                        setNoData(true);
                    }
                } else if (groupResponse.reason.response?.status === 404) {
                    setNoData(true);
                } else {
                    throw new Error("Error fetching groups");
                }

                if (employeeResponse.status === "fulfilled") {
                    setEmployees(employeeResponse.value.data);
                } else if (employeeResponse.reason.response?.status === 404) {
                    toast.info("No employees found in this organization.");
                } else {
                    throw new Error("Error fetching employees");
                }

            } catch (error) {
                console.error("Unexpected error:", error.message);
                toast.error("Some error occurred, please contact us.", {className: "toast-error"});
            }
        };

        fetchData();
    }, []);


    const handleEditClick = (employeeId) => {
        setSelectedEmployeeId(employeeId);
        const selectedEmployee = employees.find(employee => employee.id === employeeId);

        if (selectedEmployee) {
            setSelectedEmployeeData(selectedEmployee);
        } else {
            console.error('Employee not found');
        }

        setModalOn(true);
    };

    const handleAddClick = () => {
        setSelectedEmployeeId(null);
        setSelectedEmployeeData(null);
        setModalOn(true);
    };

    const handleClose = () => {
        setSelectedEmployeeId(null);
        setSelectedEmployeeData(null);
        setModalOn(false);
    };

    const handleExpandClick = (employeeId) => {
        setExpandedEmployeeId(expandedEmployeeId === employeeId ? null : employeeId);
    };

    const handleEmployeeUpdate = (updatedEmployee) => {
        setEmployees((prevEmployees) =>
            prevEmployees.map(employee =>
                employee.id === updatedEmployee.id ? updatedEmployee : employee
            )
        );
    };

    const handleEmployeeAdd = (newEmployee) => {
        setEmployees((prevEmployees) => [...prevEmployees, newEmployee]);
    };

    if (noData) {
        return (
            <div className="no-data-message">
                <h4>You don't have any group</h4>
                <p>At first create Group, only then you can add Employees</p>
                <br/>
                <div className='d-flex flex-row align-items-center gap-2'>
                    <Button variant="primary" onClick={() => navigate(`/organization/${orgId}/groups`)}>Create Group</Button>
                </div>
            </div>
        )
    }

    return (
        <div className="p-4">
            <div className='d-flex flex-row justify-content-between mb-5'>
                <h2>Employees</h2>
            </div>

            <button className="add-circle-button" onClick={handleAddClick}>+</button>

            <div className='cards-container'>
                {employees.length === 0 ? (
                    <p>You have no employees yet. Register one</p>
                ) : (
                    employees.map(employee => {
                        const isExpanded = expandedEmployeeId === employee.id;

                        return (
                            <InfoCard
                                key={employee.id}
                                className={isExpanded ? 'expanded' : ''}
                                title={`${employee.firstName} ${employee.lastName}`}
                                subtitle={employee.position}
                                text={`Group: ${groups.find(group => group.id === employee.groupId)?.name || 'N/A'}`}
                                content={
                                    isExpanded && (
                                        <div className="employee-details mt-3">
                                            <p><strong>Email:</strong> {employee.email}</p>
                                            <p><strong>Phone:</strong> {employee.phone}</p>
                                            <p><strong>Workload:</strong> {employee.workload} hours</p>
                                            <p><strong>Salary:</strong> ${employee.salary}</p>
                                            <p><strong>Hourly Rate:</strong> ${employee.salaryInHour}</p>
                                            <p><strong>Status:</strong> {
                                                employee.onVacation
                                                    ? 'On Vacation'
                                                    : employee.onSickLeave
                                                        ? 'On Sick Leave'
                                                        : 'Working'
                                            }</p>
                                        </div>
                                    )
                                }
                                actions={[
                                    {
                                        label: "Edit",
                                        variant: "secondary",
                                        onClick: () => handleEditClick(employee.id),
                                    },
                                    {
                                        label: isExpanded ? "Collapse" : "Expand",
                                        variant: "secondary",
                                        onClick: () => handleExpandClick(employee.id),
                                        icon: <FaExpand className="me-2" />
                                    }
                                ]}
                            />
                        );
                    })
                )}
            </div>

            <AsideModal
                modalOn={modalOn}
                onClose={handleClose}
                name={selectedEmployeeId ? "Edit " + (selectedEmployeeData?.firstName || "") : 'Add Employee'}
            >
                <EmployeeAddForm
                    modalOn={modalOn}
                    onClose={handleClose}
                    groups={groups}
                    employeeData={selectedEmployeeData}
                    handleEmployeeUpdate={handleEmployeeUpdate}
                    handleEmployeeAdd={handleEmployeeAdd}
                />
            </AsideModal>
        </div>
    );
};

export default Employees;
