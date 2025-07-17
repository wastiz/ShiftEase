import { Form } from 'react-bootstrap';

function TextInput({ className, label, name, type = "text", register, required, errors, ...rest }) {
    const isInvalid = !!errors?.[name];
    return (
        <Form.Group className={`input-card ${className}`}>
            {label && <Form.Label column="sm" htmlFor={name}>{label} {required && <span className="required-star">*</span>}</Form.Label>}
            <Form.Control
                type={type}
                id={name}
                placeholder="Type here..."
                isInvalid={isInvalid}
                {...register(name, { required })}
                {...rest}
            />
            {isInvalid && (
                <Form.Control.Feedback type="invalid">
                    {errors[name]?.message}
                </Form.Control.Feedback>
            )}
        </Form.Group>
    );
}

export default TextInput;
