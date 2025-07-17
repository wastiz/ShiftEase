import "./EmployeeView.css";
import { useEffect, useState } from "react";
import Cookies from "js-cookie";
import api from "../../api.js";
import { toast } from "react-toastify";
import InfoCard from "../assets/InfoCard.jsx";
import { useForm } from "react-hook-form";
import Form from "react-bootstrap/Form";
import Button from "react-bootstrap/Button";
import LoadingSpinner from "../assets/LoadingSpinner.jsx";
import TextInput from "../assets/Inputs/TextInput.jsx";

function EmployeeSickLeaveView() {
    const api_route = import.meta.env.VITE_SERVER_API;
    const token = Cookies.get("auth_token");
    const [sickLeaves, setSickLeaves] = useState([]);
    const [loading, setLoading] = useState(false);

    const {
        register,
        handleSubmit,
        reset,
        formState: { errors },
    } = useForm();

    const fetchSickLeaves = async () => {
        setLoading(true);
        try {
            const response = await api.get(`${api_route}/EmployeeOptions/get-sick-leaves`, {
                headers: {
                    Authorization: `Bearer ${token}`,
                    "Content-Type": "application/json",
                },
            });
            setSickLeaves(response.data || []);
            if (response.data.length === 0) {
                toast.warning("No sick leaves found", {className: "toast-warning"});
            }
        } catch (error) {
            console.error("Error fetching sick leaves:", error.message);
            toast.error("Failed to fetch sick leaves", {className: "toast-error"});
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchSickLeaves();
    }, []);

    const handleDelete = async (sickLeaveId) => {
        try {
            const response = await api.delete(`${api_route}/EmployeeOptions/sick-leave/${sickLeaveId}`, {
                headers: {
                    Authorization: `Bearer ${token}`,
                    "Content-Type": "application/json",
                },
            });

            if (response.status === 200) {
                setSickLeaves((prev) => prev.filter((s) => s.id !== sickLeaveId));
                toast.success("Sick leave deleted successfully!", {className: "toast-success"});
            } else {
                throw new Error(`Failed to delete: ${response.statusText}`);
            }
        } catch (error) {
            console.error("Error deleting sick leave:", error.message);
            toast.error("Failed to delete sick leave", {className: "toast-error"});
        }
    };

    const onSubmit = async (data) => {
        try {
            const sickLeaveDto = {
                startDate: data.startDate,
                endDate: data.endDate,
            };

            const response = await api.post(`${api_route}/EmployeeOptions/add-sick-leave`, sickLeaveDto, {
                headers: {
                    Authorization: `Bearer ${token}`,
                    "Content-Type": "application/json",
                },
            });

            if (response.status === 200 || response.status === 201) {
                toast.success("Sick leave added successfully!", {className: "toast-success"});
                reset();
                fetchSickLeaves();
            } else {
                throw new Error("Failed to add sick leave");
            }
        } catch (error) {
            console.error("Error adding sick leave:", error.message);
            toast.error("Failed to add sick leave", {className: "toast-error"});
        }
    };

    return (
        <div className="employee-option-view">
            <div className="cards-container">
                {loading ? (
                    <LoadingSpinner visible={loading} />
                ) : sickLeaves.length === 0 ? (
                    <p>No sick leaves found.</p>
                ) : (
                    sickLeaves.map((sickLeave) => (
                        <InfoCard
                            key={sickLeave.id}
                            title="Sick Leave"
                            content={
                                <>
                                    <p className="mb-2">
                                        <strong>Start date:</strong>{" "}
                                        {new Date(sickLeave.startDate).toLocaleDateString()}
                                    </p>
                                    <p className="mb-2">
                                        <strong>End date:</strong>{" "}
                                        {new Date(sickLeave.endDate).toLocaleDateString()}
                                    </p>
                                </>
                            }
                            actions={[
                                {
                                    label: "Remove sick leave",
                                    variant: "danger",
                                    onClick: () => handleDelete(sickLeave.id),
                                },
                            ]}
                        />
                    ))
                )}
            </div>

            <div className="form-container">
                <Form className="form-primary" onSubmit={handleSubmit(onSubmit)}>
                    <Form.Label column="lg" className="mb-3">New Sick Leave</Form.Label>
                    <TextInput
                        className="mb-3"
                        label="Start Date"
                        name="startDate"
                        type="date"
                        register={register}
                        required="Start date is required"
                        errors={errors}
                    />
                    <TextInput
                        className="mb-3"
                        label="End Date"
                        name="endDate"
                        type="date"
                        register={register}
                        required="End date is required"
                        errors={errors}
                    />
                    <Button type="submit" variant="success">
                        Send Sick Leave
                    </Button>
                </Form>
            </div>
        </div>
    );
}

export default EmployeeSickLeaveView;