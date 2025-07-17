import { useState } from 'react';
import { useForm } from 'react-hook-form';
import Form from 'react-bootstrap/Form';
import Button from 'react-bootstrap/Button';
import LoadingSpinner from '../../assets/LoadingSpinner.jsx';
import api from '../../../api.js';
import { toast } from 'react-toastify';
import TextInput from "../Inputs/TextInput.jsx";
import TextareaInput from "../Inputs/TextareaInput.jsx";

function SupportForm() {
    const api_route = import.meta.env.VITE_SERVER_API;
    const [loading, setLoading] = useState(false);

    const {
        register,
        handleSubmit,
        reset,
        formState: { errors }
    } = useForm();

    const onSubmit = async (data) => {
        setLoading(true);
        try {
            const message = {
                SenderEmail: data.senderEmail,
                Subject: data.subject,
                Message: data.message,
            };

            const response = await api.post(`${api_route}/support/send-message`, message);

            if (response.status === 200 || response.status === 201) {
                toast.success('Message sent!', {className: "toast-success"});
                reset();
            } else {
                const errText = await response.data?.text?.();
                toast.error('Error: ' + (errText || 'Unknown error'), {className: "toast-error"});
            }
        } catch (err) {
            toast.error('Error occurred: ' + err.message, {className: "toast-error"});
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="d-flex justify-content-center align-items-center" style={{ height: '100%' }}>
            <Form onSubmit={handleSubmit(onSubmit)} className="form-primary shadow-lg" style={{ maxWidth: '600px', width: '100%' }}>
                <Form.Label column="lg" className="mb-3 text-center">Contact with Support</Form.Label>
                <TextInput
                    className="mb-3"
                    label="Your email"
                    name="senderEmail"
                    type="email"
                    register={register}
                    required="Email is required"
                    errors={errors}
                />

                <TextInput
                    className="mb-3"
                    label="Topic"
                    name="subject"
                    type="text"
                    register={register}
                    required="Subject is required"
                    errors={errors}
                />

                <TextareaInput
                    className="mb-3"
                    label="Message"
                    name="message"
                    as="textarea"
                    rows={5}
                    register={register}
                    required="Message is required"
                    errors={errors}
                />

                <LoadingSpinner visible={loading} />

                <Button type="submit" variant="success" className="mb-2 mt-2">
                    Send
                </Button>
            </Form>
        </div>
    );
}

export default SupportForm;
