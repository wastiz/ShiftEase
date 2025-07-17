import { Form } from 'react-bootstrap';

function TextareaInput({ className, label, name, rows = 4, register, required, errors }) {
    const isInvalid = !!errors?.[name];
    return (
        <Form.Group className={`input-card ${className}`}>
            {label && <Form.Label column="sm" htmlFor={name}>{label} {required && <span className="required-star">*</span>}</Form.Label>}
            <Form.Control
                as="textarea"
                id={name}
                rows={rows}
                isInvalid={isInvalid}
                {...register(name, { required })}
                placeholder="Type here..."
            />
            {isInvalid && (
                <Form.Control.Feedback type="invalid">
                    {errors[name]?.message}
                </Form.Control.Feedback>
            )}
        </Form.Group>
    );
}

export default TextareaInput;
