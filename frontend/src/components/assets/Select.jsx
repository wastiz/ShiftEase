import {Form} from 'react-bootstrap'

function Select({ label, name, options, register, required, errors }) {
    return (
        <Form.Group className={'mb-3'}>
            <Form.Label htmlFor={name}>{label}</Form.Label>
            <Form.Select {...register(name, { required: required })}>
                {options.map((item) => (
                    <option key={item.id} value={item.id}>
                        {item.name}
                    </option>
                ))}
            </Form.Select>
            {errors[name] && (
                <p className="text-warning">{errors[name].message}</p>
            )}
        </Form.Group>
    );
}

export default Select;