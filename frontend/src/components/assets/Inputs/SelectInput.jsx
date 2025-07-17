import { Form } from 'react-bootstrap';

function SelectInput({ className, label, name, options, register, required, errors }) {
    const isInvalid = !!errors?.[name];
    return (
        <Form.Group className={`input-card ${className}`}>
            {label && <Form.Label column="sm" htmlFor={name}>{label} {required && <span className="required-star">*</span>}</Form.Label>}
            <Form.Select
                id={name}
                isInvalid={isInvalid}
                {...register(name, { required })}
            >
                {options.map((item) => (
                    <option key={item.id} value={item.id}>
                        {item.name}
                    </option>
                ))}
            </Form.Select>
            {isInvalid && (
                <Form.Control.Feedback type="invalid">
                    {errors[name]?.message}
                </Form.Control.Feedback>
            )}
        </Form.Group>
    );
}

export default SelectInput;
