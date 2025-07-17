import "./EmployeeView.css";
import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import api from "../../api.js";
import { toast } from "react-toastify";
import InfoCard from "../assets/InfoCard.jsx";
import Form from "react-bootstrap/Form";
import Button from "react-bootstrap/Button";
import LoadingSpinner from "../assets/LoadingSpinner.jsx";
import TextInput from "../assets/Inputs/TextInput.jsx";

function EmployeeVacationView() {
    const api_route = import.meta.env.VITE_SERVER_API;
    const [vacationRequests, setVacationRequests] = useState([]);
    const [loading, setLoading] = useState(false);

    const {
        register,
        handleSubmit,
        reset,
        formState: { errors },
    } = useForm();

    const fetchVacationsRequests = async () => {
        setLoading(true);
        try {
            const response = await api.get(`${api_route}/EmployeeOptions/get-vacations`);
            setVacationRequests(response.data || []);
            if (response.data.length === 0) {
                toast.warning("No vacations found", {className: "toast-warning"});
            }
        } catch (error) {
            console.error("Error fetching vacation requests:", error.message);
            toast.error("Failed to fetch vacation requests", {className: "toast-error"});
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchVacationsRequests();
    }, []);

    const handleDelete = async (vacationRequestId) => {
        try {
            const response = await api.delete(`${api_route}/EmployeeOptions/delete-vacation/${vacationRequestId}`);
            if (response.status === 200) {
                setVacationRequests(vacationRequests.filter(v => v.id !== vacationRequestId));
                toast.success("Vacation request deleted successfully!", {className: "toast-success"});
            } else {
                throw new Error(`Failed to delete: ${response.statusText}`);
            }
        } catch (error) {
            console.error("Error deleting vacation request:", error.message);
            toast.error("Failed to delete vacation request", {className: "toast-error"});
        }
    };

    const onSubmit = async (data) => {
        try {
            const vacationRequestDto = {
                startDate: data.startDate,
                endDate: data.endDate,
            };

            const response = await api.post(`${api_route}/EmployeeOptions/add-vacation-request`, vacationRequestDto);

            if (response.status === 200 || response.status === 201) {
                toast.success("Vacation request added successfully!", {className: "toast-success"});
                reset();
                fetchVacationsRequests();
            } else {
                throw new Error("Failed to add vacation request");
            }
        } catch (error) {
            console.error("Error adding vacation request:", error.message);
            toast.error("Failed to add vacation request", {className: "toast-error"});
        }
    };

    return (
        <div className="employee-option-view">
            <div className="cards-container">
                {loading ? (
                    <LoadingSpinner visible={true} />
                ) : vacationRequests.length === 0 ? (
                    <p>No vacation requests found.</p>
                ) : (
                    vacationRequests.map(request => (
                        <InfoCard
                            key={request.id}
                            title="Vacation Request"
                            content={
                                <>
                                    <p className="mb-2">
                                        <strong>Start date:</strong> {new Date(request.startDate).toLocaleDateString()}
                                    </p>
                                    <p className="mb-2">
                                        <strong>End date:</strong> {new Date(request.endDate).toLocaleDateString()}
                                    </p>
                                    <p className="mb-2">
                                        <strong>Status:</strong>{" "}
                                        {request.accepted
                                            ? "Accepted"
                                            : request.rejected
                                                ? "Rejected"
                                                : "Pending"}
                                    </p>
                                </>
                            }
                            actions={[
                                {
                                    label: "Remove request",
                                    variant: "danger",
                                    onClick: () => handleDelete(request.id),
                                },
                            ]}
                        />
                    ))
                )}
            </div>

            <div className="form-container">
                <Form className="form-primary" onSubmit={handleSubmit(onSubmit)}>
                    <Form.Label column="lg" className="mb-3">New Vacation Request</Form.Label>
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
                    <Button variant="success" type="submit" className="mt-3">
                        Send Request
                    </Button>
                </Form>
            </div>
        </div>
    );
}

export default EmployeeVacationView;