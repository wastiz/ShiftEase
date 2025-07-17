import { useEffect } from "react";
import { useForm } from "react-hook-form";
import api from "../../../api.js";
import Input from "../../assets/Input.jsx";
import Cookies from "js-cookie";
import { toast } from "react-toastify";
import TextInput from "../../assets/Inputs/TextInput.jsx";
import {Card} from "react-bootstrap";
import Button from "react-bootstrap/Button";
import ColorInput from "../../assets/Inputs/ColorInput.jsx";

const ShiftTypeForm = ({ onClose, shiftTypeData, onCreate, onUpdate, onDelete }) => {
    const token = Cookies.get("auth_token");

    const { register, handleSubmit, setValue, reset, watch, formState: { errors } } = useForm();

    useEffect(() => {
        if (shiftTypeData) {
            setValue('name', shiftTypeData.name);
            setValue('startTime', shiftTypeData.startTime);
            setValue('endTime', shiftTypeData.endTime);
            setValue('employeeNeeded', shiftTypeData.employeeNeeded);
            setValue('color', shiftTypeData.color);
        } else {
            reset();
        }
    }, [shiftTypeData, setValue, reset]);

    const onSubmit = async (data) => {
        try {
            const payload = {
                ...data,
                employeeNeeded: parseInt(data.employeeNeeded, 10),
            };

            const config = {
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${token}`
                }
            };

            let response;
            if (shiftTypeData) {
                response = await api.put(`/ShiftType/${shiftTypeData.id}`, payload, config);
                toast.success("Shift type updated successfully!", {className: "toast-success"});
                onUpdate(response.data);
            } else {
                response = await api.post(`/ShiftType/organization`, payload, config);
                toast.success("Shift type created successfully!", {className: "toast-success"});
                onCreate(response.data);
            }

            onClose();
        } catch (error) {
            console.error("Error submitting data", error);
            toast.error("An error occurred while submitting the data.", {className: "toast-error"});
        }
    };

    const handleDelete = async () => {
        if (!shiftTypeData?.id) return;

        try {
            await api.delete(`/ShiftType/${shiftTypeData.id}`, {
                headers: {
                    Authorization: `Bearer ${token}`
                }
            });
            toast.success("Shift type deleted successfully!");
            onDelete(shiftTypeData.id);
            onClose();
        } catch (error) {
            console.error("Error deleting shift type", error);
            toast.error("Failed to delete shift type.", {className: "toast-error"});
        }
    };

    return (
        <form className="form-primary" onSubmit={handleSubmit(onSubmit)} key={shiftTypeData?.id || 'new'}>
            <TextInput
                className="mb-3"
                label="Shift Name"
                name="name"
                type="text"
                register={register}
                required={true}
                errors={errors}
            />
            <TextInput
                className="mb-3"
                label="Start Time"
                name="startTime"
                type="time"
                register={register}
                required={true}
                errors={errors}
            />
            <TextInput
                className="mb-3"
                label="End Time"
                name="endTime"
                type="time"
                register={register}
                required={true}
                errors={errors}
            />
            <TextInput
                className="mb-3"
                label="Employee Needed"
                name="employeeNeeded"
                type="number"
                register={register}
                required={true}
                errors={errors}
            />

            <ColorInput
                className="mb-3"
                label="Shift color"
                name="color"
                register={register}
                required={false}
                errors={errors}
            />

            <Card.Footer>
                <Button type="submit" variant="success">
                    {shiftTypeData ? "Update Shift" : "Create Shift"}
                </Button>
                {shiftTypeData && (
                    <Button type="button" variant="danger" onClick={handleDelete}>
                        Delete
                    </Button>
                )}
            </Card.Footer>
        </form>
    );
};

export default ShiftTypeForm;
