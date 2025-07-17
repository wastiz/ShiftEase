import { useEffect, useState } from "react";
import "./OrganizationAddForm.css";
import { useNavigate, useParams } from "react-router-dom";
import { useForm } from "react-hook-form";
import Input from '../../assets/Input.jsx';
import { toast } from 'react-toastify';
import { Form, Row, Col } from 'react-bootstrap';
import HolidaySelector from "./HolidaySelector.jsx";
import api from "../../../api.js";
import Button from "react-bootstrap/Button";
import ToggleGroup from "../../assets/ToggleGroup.jsx";
import TextInput from "../../assets/Inputs/TextInput.jsx";
import TextareaInput from "../../assets/Inputs/TextareaInput.jsx";
import SelectInput from "../../assets/Inputs/SelectInput.jsx";

function transformWorkDays(input) {
    const workDaysList = [];

    for (const [dayName, dayData] of Object.entries(input)) {
        if (dayData.active) {
            workDaysList.push({
                weekDayName: dayName,
                from: dayData.from,
                to: dayData.to
            });
        }
    }

    return workDaysList;
}

const OrganizationAddForm = () => {
    const { register, handleSubmit, setValue, reset, formState: { errors }, watch } = useForm();
    const { orgId } = useParams();
    const navigate = useNavigate();
    const [isLoading, setIsLoading] = useState(false);
    const [selectedHolidays, setSelectedHolidays] = useState([]);
    const [scheduleMode, setScheduleMode] = useState("manual");

    const [workDays, setWorkDays] = useState({
        monday: { active: false, from: '08:00', to: '17:00' },
        tuesday: { active: false, from: '08:00', to: '17:00' },
        wednesday: { active: false, from: '08:00', to: '17:00' },
        thursday: { active: false, from: '08:00', to: '17:00' },
        friday: { active: false, from: '08:00', to: '17:00' },
        saturday: { active: false, from: '08:00', to: '17:00' },
        sunday: { active: false, from: '08:00', to: '17:00' }
    });
    const [is24_7, setIs24_7] = useState(false);

    const handleScheduleModeChange = (val) => {
        setScheduleMode(val);

        if (val === "fiveTwo") {
            setIs24_7(false);
            apply5DayTemplate();
        } else if (val === "fullTime") {
            setIs24_7(true);
            setWorkDays({
                monday: { active: true, from: '00:00', to: '23:59' },
                tuesday: { active: true, from: '00:00', to: '23:59' },
                wednesday: { active: true, from: '00:00', to: '23:59' },
                thursday: { active: true, from: '00:00', to: '23:59' },
                friday: { active: true, from: '00:00', to: '23:59' },
                saturday: { active: true, from: '00:00', to: '23:59' },
                sunday: { active: true, from: '00:00', to: '23:59' }
            });
        } else {
            setIs24_7(false);
            setWorkDays(prev => {
                const resetDays = {};
                Object.keys(prev).forEach(day => {
                    resetDays[day] = { ...prev[day], active: false };
                });
                return resetDays;
            });
        }
    };

    const apply5DayTemplate = () => {
        setWorkDays({
            monday: { active: true, from: '08:00', to: '17:00' },
            tuesday: { active: true, from: '08:00', to: '17:00' },
            wednesday: { active: true, from: '08:00', to: '17:00' },
            thursday: { active: true, from: '08:00', to: '17:00' },
            friday: { active: true, from: '08:00', to: '17:00' },
            saturday: { active: false, from: '08:00', to: '17:00' },
            sunday: { active: false, from: '08:00', to: '17:00' }
        });
    };

    useEffect(() => {
        if (!orgId) return;

        const fetchOrganization = async () => {
            try {
                const response = await api.get(`/organization/${orgId}`);

                if (response.status !== 200) {
                    throw new Error("Failed to fetch organization");
                }

                const org = await response.data;

                reset({
                    name: org.organizationName,
                    description: org.description,
                    organizationType: org.organizationType,
                    website: org.website,
                    phone: org.phone,
                    address: org.address,
                    photoUrl: org.photoUrl,
                    nightShiftBonus: org.nightShiftBonus,
                    holidayBonus: org.holidayBonus,
                    employeeCount: org.employeeCount
                });

                if (org.isOpen24_7) {
                    setIs24_7(true);
                } else if (org.workDays) {
                    const reconstructedWorkDays = {};
                    for (const day of org.workDays) {
                        reconstructedWorkDays[day.weekDayName.toLowerCase()] = {
                            active: true,
                            from: day.from,
                            to: day.to
                        };
                    }
                    setWorkDays(prev => ({
                        ...prev,
                        ...reconstructedWorkDays
                    }));
                }

                if (org.organizationHolidays) {
                    setSelectedHolidays(org.organizationHolidays);
                }

            } catch (error) {
                console.error("Error fetching organization:", error);
                toast.error("Failed to load organization data", {className: "toast-error"});
            }
        };

        fetchOrganization();
    }, [orgId, reset]);


    const onSubmit = async (data) => {
        setIsLoading(true);

        try {
            const transformedWorkDays = transformWorkDays(workDays);
            const organizationDto = {
                name: data.name,
                description: data.description,
                organizationType: data.organizationType || "General",
                isOpen24_7: is24_7,
                nightShiftBonus: parseFloat(data.nightShiftBonus || 0),
                holidayBonus: parseFloat(data.holidayBonus || 0),
                photoUrl: data.photoUrl || "",
                employeeCount: parseInt(data.employeeCount || 0),
                website: data.website,
                phone: data.phone,
                address: data.address,
                organizationWorkDays: is24_7 ? null : transformedWorkDays,
                organizationHolidays: selectedHolidays,
            };

            let response;

            if (orgId) {
                response = await api.put(`/organization/${orgId}`, organizationDto);
            } else {
                response = await api.post(`/organization`, organizationDto);
            }

            if (response.status !== 200) {
                throw new Error("Failed to save organization");
            }

            toast.success(orgId ? "Organization updated successfully!" : "Organization created successfully!");
            navigate(`/organizations`);
        } catch (error) {
            console.error("Error saving organization:", error);
            toast.error("Failed to save organization", {className: "toast-error"});
        } finally {
            setIsLoading(false);
        }
    };

    const handleDayChange = (day) => {
        setWorkDays(prev => ({
            ...prev,
            [day]: { ...prev[day], active: !prev[day].active }
        }));
    };

    const handleTimeChange = (day, field, value) => {
        setWorkDays(prev => ({
            ...prev,
            [day]: { ...prev[day], [field]: value }
        }));
    };

    return (
        <div className="organization-form-container">
            <form onSubmit={handleSubmit(onSubmit)}>
                <Row>
                    <Col md={6} className="required-fields">
                        <h3 className="section-title">Basic Information <span className="required-star">*</span></h3>

                        <TextInput
                            className="mb-3"
                            label="Organization Name"
                            name="name"
                            type="text"
                            register={register}
                            required
                            errors={errors}
                        />

                        <TextareaInput
                            className="mb-3"
                            label="Description"
                            name="description"
                            as="textarea"
                            rows={3}
                            register={register}
                            required="Description is required"
                            errors={errors}
                        />

                        <SelectInput
                            className="mb-3"
                            label="Organization Type"
                            name="organizationType"
                            as="select"
                            register={register}
                            required="Organization type is required"
                            errors={errors}
                            options={[
                                { id: "Retail", name: "Retail" },
                                { id: "Hospitality", name: "Hospitality" },
                                { id: "Healthcare", name: "Healthcare" },
                                { id: "Manufacturing", name: "Manufacturing" },
                                { id: "Other", name: "Other" },
                            ]}
                        />

                        <div className="input-card">
                            <Form.Label column="sm">Work Schedule <span className="required-star">*</span></Form.Label>

                            <ToggleGroup
                                className="mt-3"
                                name="scheduleMode"
                                value={scheduleMode}
                                onChange={handleScheduleModeChange}
                                idPrefix="schedule"
                                options={[
                                    { label: "Manual", value: "manual" },
                                    { label: "5/2 Work Week", value: "fiveTwo" },
                                    { label: "24/7 Operation", value: "fullTime" },
                                ]}
                            />

                            {!is24_7 && (
                                <div className="days-selector mt-3">
                                    {Object.entries(workDays).map(([day, config]) => (
                                        <div key={day} className="day-row mb-2">
                                            <Form.Check
                                                type="checkbox"
                                                label={day.charAt(0).toUpperCase() + day.slice(1)}
                                                checked={config.active}
                                                onChange={() => handleDayChange(day)}
                                            />

                                            {config.active && (
                                                <div className="time-inputs">
                                                    <Form.Control
                                                        type="time"
                                                        value={config.from}
                                                        onChange={(e) => handleTimeChange(day, 'from', e.target.value)}
                                                        disabled={is24_7}
                                                    />
                                                    <span className="time-separator">to</span>
                                                    <Form.Control
                                                        type="time"
                                                        value={config.to}
                                                        onChange={(e) => handleTimeChange(day, 'to', e.target.value)}
                                                        disabled={is24_7}
                                                    />
                                                </div>
                                            )}
                                        </div>
                                    ))}
                                </div>
                            )}
                        </div>
                    </Col>

                    <Col md={6} className="optional-fields">
                        <h3 className="section-title">Additional Information</h3>

                        <TextInput
                            className="mb-3"
                            label="Website"
                            name="website"
                            type="url"
                            register={register}
                            errors={errors}
                        />

                        <TextInput
                            className="mb-3"
                            label="Phone Number"
                            name="phone"
                            type="tel"
                            register={register}
                            errors={errors}
                        />

                        <TextInput
                            className="mb-3"
                            label="Address"
                            name="address"
                            type="text"
                            register={register}
                            errors={errors}
                        />

                        <TextInput
                            className="mb-3"
                            label="Photo URL"
                            name="photoUrl"
                            type="text"
                            register={register}
                            errors={errors}
                        />

                        <TextInput
                            className="mb-3"
                            label="Night Shift Bonus (%)"
                            name="nightShiftBonus"
                            type="number"
                            register={register}
                            errors={errors}
                        />

                        <TextInput
                            className="mb-3"
                            label="Holiday Bonus (%)"
                            name="holidayBonus"
                            type="number"
                            register={register}
                            errors={errors}
                        />

                        <TextInput
                            className="mb-3"
                            label="Employee Count (approximate)"
                            name="employeeCount"
                            type="number"
                            register={register}
                            errors={errors}
                        />

                        <HolidaySelector
                            selectedHolidays={selectedHolidays}
                            setSelectedHolidays={setSelectedHolidays}
                        />
                    </Col>
                </Row>

                <div className="card-footer">
                    <Button variant="success" type="submit" disabled={isLoading}>
                        {isLoading ? "Saving..." : (orgId ? "Save Changes" : "Create Organization")}
                    </Button>
                    <button className="btn btn-secondary me-2" onClick={() => navigate(-1)}>
                        Cancel
                    </button>
                </div>
            </form>
        </div>
    );
};

export default OrganizationAddForm;
