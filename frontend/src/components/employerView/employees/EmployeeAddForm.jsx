import './EmployeeAddForm.css';
import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { toast } from 'react-toastify';
import Input from '../../assets/Input.jsx';
import Select from '../../assets/Select.jsx';
import api from '../../../api.js';
import Form from "react-bootstrap/Form";
import Button from "react-bootstrap/Button";
import TextInput from "../../assets/Inputs/TextInput.jsx";
import SelectInput from "../../assets/Inputs/SelectInput.jsx";
import {Card} from "react-bootstrap";

const EmployeeAddForm = ({ onClose, groups, employeeData, handleEmployeeUpdate, handleEmployeeAdd }) => {
    const api_route = import.meta.env.VITE_SERVER_API;
    const { register, handleSubmit, setValue, reset, formState: { errors } } = useForm();

    useEffect(() => {
        if (employeeData) {
            setValue('firstName', employeeData.firstName);
            setValue('lastName', employeeData.lastName);
            setValue('email', employeeData.email);
            setValue('phone', employeeData.phone);
            setValue('password', employeeData.password);
            setValue('position', employeeData.position);
            setValue('workingHours', employeeData.workload);
            setValue('monthlySalary', employeeData.salary);
            setValue('hourlyRate', employeeData.salaryInHour);
            setValue('shiftPriority', employeeData.priority.toLowerCase());
            setValue('groupId', employeeData.groupId);
        } else {
            reset();
        }
    }, [employeeData, setValue, reset]);

    const onSubmit = async (data) => {
        const employee = {
            firstName: data.firstName,
            lastName: data.lastName,
            email: data.email,
            phone: data.phone,
            password: data.password,
            position: data.position,
            workload: parseFloat(data.workingHours),
            salary: parseFloat(data.monthlySalary),
            salaryInHour: parseFloat(data.hourlyRate),
            priority: data.shiftPriority,
            groupId: parseInt(data.groupId),
        };

        console.log(employee)

        try {
            let response;

            if (employeeData) {
                response = await api.put(`${api_route}/employee/${employeeData.id}`, employee);
                toast.success(response.data.message);
                handleEmployeeUpdate(employee);
            } else {
                response = await api.post(`${api_route}/employee/`, employee);
                toast.success(response.data.message, {className: "toast-success"});
                handleEmployeeAdd(employee);
            }
        } catch (error) {
            if (error.response && error.response.data && error.response.data.message) {
                toast.error(error.response.data.message, {className: "toast-error"});
            } else {
                toast.error("Server error occurred. Please contact us!", {className: "toast-error"});
            }
        }

        onClose();
    };

    return (
        <Form onSubmit={handleSubmit(onSubmit)}>
            <TextInput
                className="mb-3"
                label="First Name"
                name="firstName"
                type="text"
                register={register}
                required
                errors={errors}
            />
            <TextInput
                className="mb-3"
                label="Last Name"
                name="lastName"
                type="text"
                register={register}
                required
                errors={errors}
            />
            <TextInput
                className="mb-3"
                label="Email"
                name="email"
                type="email"
                register={register}
                required
                errors={errors}
            />
            <TextInput
                className="mb-3"
                label="Phone"
                name="phone"
                type="tel"
                register={register}
                required
                errors={errors}
            />
            <TextInput
                className="mb-3"
                label="Password"
                name="password"
                type="password"
                register={register}
                required
                errors={errors}
            />
            <TextInput
                className="mb-3"
                label="Position"
                name="position"
                type="text"
                register={register}
                required
                errors={errors}
            />
            <TextInput
                className="mb-3"
                label="Working Hours"
                name="workingHours"
                type="number"
                register={register}
                required
                errors={errors}
            />
            <TextInput
                className="mb-3"
                label="Monthly Salary"
                name="monthlySalary"
                type="number"
                register={register}
                required
                errors={errors}
            />
            <TextInput
                className="mb-3"
                label="Hourly Rate"
                name="hourlyRate"
                type="number"
                register={register}
                required
                errors={errors}
            />
            <SelectInput
                className="mb-3"
                label="Shift Priority"
                name="shiftPriority"
                register={register}
                required
                errors={errors}
                options={[{id: "high", name: "High"}, {id: "medium", name: "Medium"}, {id: "low", name: "Low"}]}
            />
            <SelectInput
                className="mb-3"
                label="Group"
                name="groupId"
                options={groups}
                register={register}
                required
                errors={errors}
            />
            <Card.Footer>
                <Button type="submit" variant="success">Save</Button>
            </Card.Footer>
        </Form>
    );
};

export default EmployeeAddForm;
