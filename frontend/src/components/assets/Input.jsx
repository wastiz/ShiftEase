import { Form } from 'react-bootstrap';

function Input({
                   label,
                   name,
                   type = "text",
                   as = "input",
                   rows,
                   options = [],
                   register,
                   required,
                   errors,
                   onChange,
               }) {
    const isInvalid = !!errors[name];

    const registration = register(name, {
        required,
        onChange,
    });

    return (
        <Form.Group className="input-card">
            <Form.Label column="sm" htmlFor={name}>
                {label} {required && <span className="required-star">*</span>}
            </Form.Label>
            {as === 'select' ? (
                <Form.Select
                    id={name}
                    isInvalid={isInvalid}
                    {...registration}
                >
                    {options.map((option, index) => (
                        <option key={index} value={option.value}>
                            {option.label}
                        </option>
                    ))}
                </Form.Select>
            ) : (
                <Form.Control
                    as={as}
                    rows={rows}
                    type={type}
                    id={name}
                    placeholder="Type here..."
                    isInvalid={isInvalid}
                    {...registration}
                />
            )}

            <Form.Control.Feedback type="invalid">
                {errors[name]?.message}
            </Form.Control.Feedback>
        </Form.Group>
    );
}

export default Input;
