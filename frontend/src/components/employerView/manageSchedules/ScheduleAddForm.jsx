import './ScheduleAddForm.css';
import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import Input from '../../assets/Input.jsx';
import Select from '../../assets/Select.jsx';

function ScheduleAddForm({ onClose, scheduleData, groups, shiftTypes }) {
    const { register, handleSubmit, setValue, reset, control, formState: { errors } } = useForm();
    const [selectedShiftTypes, setSelectedShiftTypes] = useState([]);

    useEffect(() => {
        if (scheduleData) {
            setValue('name', scheduleData.name);
            setValue('start_date', scheduleData.start_date);
            setValue('end_date', scheduleData.end_date);
            setValue('group', scheduleData.group);
            setValue('group_description', scheduleData.group_description);
            setSelectedShiftTypes(scheduleData.shiftTypes || []);
        } else {
            reset();
        }
    }, [scheduleData, setValue, reset]);

    const onSubmit = (data) => {
        console.log({ ...data, shiftTypes: selectedShiftTypes });
        onClose();
    };

    const toggleShiftType = (type) => {
        if (selectedShiftTypes.includes(type)) {
            setSelectedShiftTypes(selectedShiftTypes.filter(t => t !== type));
        } else {
            setSelectedShiftTypes([...selectedShiftTypes, type]);
        }
    };

    return (
        <form onSubmit={handleSubmit(onSubmit)}>
            <Input 
                label="Schedule Name" 
                name="name" 
                type="text" 
                register={register} 
                required 
                errors={errors} 
            />
            <Input 
                label="Start Date" 
                name="start_date" 
                type="datetime-local" 
                register={register} 
                required 
                errors={errors} 
            />
            <Input 
                label="End Date" 
                name="end_date" 
                type="datetime-local" 
                register={register} 
                required 
                errors={errors} 
            />
            <Select 
                label="Group" 
                name="group" 
                options={groups.map(group => group.name)} 
                register={register} 
                required 
                errors={errors} 
                control={control}
            />
            <div>
                <h4>Available Shift Types:</h4>
                <ul>
                    {shiftTypes.map(shiftType => (
                        <li key={shiftType.name}>
                            <span style={{ color: selectedShiftTypes.includes(shiftType.name) ? 'var(--coral)' : 'inherit' }}>
                                {shiftType.name}
                            </span>
                            <button 
                                type="button" 
                                style={{ marginLeft: '8px', fontSize: '0.7em', padding: '2px 4px', width: '20%' }}
                                onClick={() => toggleShiftType(shiftType.name)}
                            >
                                {selectedShiftTypes.includes(shiftType.name) ? 'Remove' : 'Add'}
                            </button>
                        </li>
                    ))}
                </ul>
            </div>
            <button type="submit" className='btn btn-primary'>{scheduleData ? 'Update' : 'Add'}</button>
        </form>
    );
}

export default ScheduleAddForm;