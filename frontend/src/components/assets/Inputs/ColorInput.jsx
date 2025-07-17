import { Form } from 'react-bootstrap';

function ColorInput({ className, label, name, register, required, errors }) {
    const isInvalid = !!errors?.[name];

    return (
        <Form.Group className={`input-card ${className}`}>
            {label && <Form.Label column="sm" htmlFor={name}>{label} {required && <span className="required-star">*</span>}</Form.Label>}
            <Form.Control
                type="color"
                id={name}
                {...register(name, { required })}
            />
            {isInvalid && (
                <Form.Control.Feedback type="invalid">
                    {errors[name]?.message}
                </Form.Control.Feedback>
            )}
        </Form.Group>
    );
}

export default ColorInput;
