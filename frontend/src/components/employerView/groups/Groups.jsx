import { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { toast } from 'react-toastify';
import api from '../../../api.js';
import './Groups.css';
import Modal from "react-bootstrap/Modal";
import Button from "react-bootstrap/Button";
import Form from "react-bootstrap/Form";
import InfoCard from "../../assets/InfoCard.jsx";
import AsideModal from "../../assets/asideModal/AsideModal.jsx";
import TextInput from "../../assets/Inputs/TextInput.jsx";
import TextareaInput from "../../assets/Inputs/TextareaInput.jsx";
import ColorInput from "../../assets/Inputs/ColorInput.jsx";
import {Card} from "react-bootstrap";

function Groups() {
    const [groups, setGroups] = useState([]);
    const [editingGroupId, setEditingGroupId] = useState(null);
    const [isFormModalOpen, setIsFormModalOpen] = useState(false);
    const [confirmDeleteId, setConfirmDeleteId] = useState(null);

    const {
        register,
        handleSubmit,
        reset,
        setValue,
        formState: { errors }
    } = useForm();

    useEffect(() => {
        const fetchGroups = async () => {
            try {
                const response = await api.get(`/group/by-organization-id`);
                if (response.status === 200) {
                    setGroups(response.data);
                } else {
                    throw new Error(`Failed to fetch: ${response.statusText}`);
                }
            } catch (error) {
                console.error("Error fetching groups:", error.message);
            }
        };
        fetchGroups();
    }, [api]);

    const handleModalClose = () => setConfirmDeleteId(null);
    const handleModalShow = (groupId) => setConfirmDeleteId(groupId);

    const openFormModal = (group = null) => {
        if (group) {
            setEditingGroupId(group.id);
            reset({
                name: group.name,
                description: group.description,
                color: group.color || '#ffffff'
            });
        } else {
            setEditingGroupId(null);
            reset({
                name: '',
                description: '',
                color: '#ffffff'
            });
        }
        setIsFormModalOpen(true);
    };

    const closeFormModal = () => {
        setIsFormModalOpen(false);
        setEditingGroupId(null);
        reset();
    };

    const onSubmit = async (data) => {
        try {
            const payload = {
                ...data,
                organizationId: parseInt(localStorage.getItem("orgId"))
            };

            const response = editingGroupId
                ? await api.put(`/group/${editingGroupId}`, payload)
                : await api.post(`/group`, payload);

            if (![200, 201].includes(response.status)) throw new Error("Failed to save group");

            toast.success(editingGroupId ? "Group updated successfully!" : "Group created successfully!", {className: "toast-success"});

            if (editingGroupId) {
                setGroups(groups.map(group =>
                    group.id === editingGroupId ? { ...group, ...payload } : group
                ));
            } else {
                setGroups([...groups, response.data]);
            }

            closeFormModal();
        } catch (error) {
            console.error("Error saving group:", error);
            toast.error("Failed to save group", {className: "toast-error"});
        }
    };

    const handleDelete = async (groupId) => {
        try {
            const response = await api.delete(`/group/${groupId}`);
            if (response.status === 200) {
                setGroups(groups.filter(group => group.id !== groupId));
                toast.success('Group deleted successfully!', {className: "toast-success"});
            } else {
                throw new Error(`Failed to delete: ${response.statusText}`);
            }
        } catch (error) {
            console.error("Error deleting group:", error.message);
            toast.error("Failed to delete group", {className: "toast-error"});
        }
        handleModalClose();
    };

    return (
        <div className="p-4">
            {groups.length > 0 ? (
                <>
                    <h2 className="mb-5">Groups</h2>
                    <div className="cards-container">
                        {groups.map(group => (
                            <InfoCard
                                style={{borderColor: group.color}}
                                key={group.id}
                                title={group.name}
                                subtitle={group.description}
                                actions={[
                                    {
                                        label: "Edit",
                                        variant: "secondary",
                                        onClick: () => openFormModal(group)
                                    }
                                ]}
                            />
                        ))}
                    </div>
                    <button className="add-circle-button" onClick={() => openFormModal()}>+</button>
                </>
            ) : (
                <div className="no-data-message">
                    <h4>You have no groups yet</h4>
                    <Button variant="primary" onClick={() => openFormModal()}>
                        Create your first group
                    </Button>
                </div>
            )}

            {/* Aside Modal with Form */}
            <AsideModal
                modalOn={isFormModalOpen}
                onClose={closeFormModal}
                name={editingGroupId ? "Edit Group" : "Add Group"}
            >
                <Form onSubmit={handleSubmit(onSubmit)}>
                    <TextInput
                        className="mb-3"
                        label="Group name"
                        name="name"
                        register={register}
                        required="Group name is required"
                        errors={errors}
                    />
                    <TextareaInput
                        className="mb-3"
                        label="Group description"
                        name="description"
                        register={register}
                        required={false}
                        errors={errors}
                    />
                    <ColorInput
                        className="mb-3"
                        label="Pick color"
                        name="color"
                        register={register}
                        required={false}
                        errors={errors}
                    />
                    <Card.Footer>
                        <Button variant="success" type="submit">
                            Save
                        </Button>
                        {editingGroupId && (
                            <Button variant="danger" type="button"
                                    onClick={() => {
                                    handleModalShow(editingGroupId);
                                    closeFormModal();
                            }}>Delete</Button>
                        )}
                    </Card.Footer>
                </Form>
            </AsideModal>

            {/* Delete confirmation modal */}
            <Modal show={!!confirmDeleteId} onHide={handleModalClose}>
                <Modal.Header closeButton>
                    <Modal.Title>Delete Group</Modal.Title>
                </Modal.Header>
                <Modal.Body>Are you sure?</Modal.Body>
                <Modal.Footer>
                    <Button variant="secondary" onClick={handleModalClose}>Close</Button>
                    <Button variant="danger" onClick={() => handleDelete(confirmDeleteId)}>Delete Group</Button>
                </Modal.Footer>
            </Modal>
        </div>
    );
}

export default Groups;
