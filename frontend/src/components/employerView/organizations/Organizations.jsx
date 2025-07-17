import { useState, useEffect } from "react";
import {useNavigate} from "react-router-dom";
import "./Organizations.css";
import { toast } from 'react-toastify';
import api from "../../../api.js";
import Modal from "react-bootstrap/Modal";
import Button from "react-bootstrap/Button";
import InfoCard from "../../assets/InfoCard.jsx";
import {LuLogOut} from "react-icons/lu";
import {Card} from "react-bootstrap";

const Organizations = () => {
    const navigate = useNavigate();
    const [organizations, setOrganizations] = useState([]);
    //For Modal
    const [show, setShow] = useState(false);
    const [orgIdToDelete, setOrgIdToDelete] = useState(null);

    const handleModalClose = () => setShow(false);
    const handleModalShow = (orgId) => {
        setOrgIdToDelete(orgId);
        setShow(true);
    };

    useEffect(() => {
        const fetchOrganizations = async () => {

            try {
                const response = await api.get(`/organization/by-employer-id`);

                if (response.status !== 200) {
                    throw new Error(`Failed to fetch: ${response.statusText}`);
                }

                setOrganizations(response.data || []);
            } catch (error) {
                toast.error(error.message, {className: "toast-error"});
                console.error("Error fetching organizations:", error.message);
            }
        };

        fetchOrganizations();
    }, []);

    const handleDeleteClick = async () => {
        try {
            await api.delete(`/organization/${orgIdToDelete}`);

            toast.success('Organization deleted successfully', {className: "toast-success"});
            handleModalClose();
            setOrganizations(organizations.filter(org => org.id !== orgIdToDelete));
        } catch (error) {
            console.error("Deleting organization error:", error.message);
            toast.error('Failed to delete organization', {className: "toast-error"});
        }
    };


    const handleLogOut = () => {
        localStorage.removeItem("accessToken");
        localStorage.removeItem("refreshToken");
        navigate("/login");
    }

    const handleNavigate = (orgId) => {
        localStorage.setItem("orgId", orgId);
        navigate(`/organization/${orgId}/shift-table`)
    }

    return (
        <div className="organizations-page">
            <h1 className="organizations-header">Your Organizations<LuLogOut onClick={handleLogOut} className="link-text" /> </h1>

            {organizations.length > 0 ? (
                <>
                <div className="cards-container p-4">
                    {organizations.map((organization) => (
                        <Card >
                            <Card.Body>
                                {organization.photoUrl ? (
                                    <Card.Img src={organization.photoUrl} />
                                ) : (
                                    <div className="card-img">
                                        <svg
                                            xmlns="http://www.w3.org/2000/svg"
                                            viewBox="0 0 55.61 55.61"
                                        >
                                            <style>
                                                {`
                                                  .cls-1 {
                                                    stroke: var(--svg-stroke-color, var(--text-primary));
                                                    fill: var(--svg-fill-color, var(--bg-tertiary));
                                                    stroke-linecap: round;
                                                    stroke-linejoin: round;
                                                    stroke-width: 2px;
                                                  }
                                                `}
                                            </style>
                                            <g id="Layer_2" data-name="Layer 2">
                                                <g id="Layer_1-2" data-name="Layer 1">
                                                    <path className="cls-1" d="M54.61,9V46.88h0L35.92,31.31,23.82,21.23a4.91,4.91,0,0,0-6.51.19L1,36.73V9A8,8,0,0,1,9,1H46.57A8,8,0,0,1,54.61,9Z"/>
                                                    <path className="cls-1" d="M54.61,46.88a8,8,0,0,1-8,7.73H9a8,8,0,0,1-8-8V36.73L17.31,21.42a4.91,4.91,0,0,1,6.51-.19l12.1,10.08Z"/>
                                                    <path className="cls-1" d="M54.61,33.58v13l-.23.11L35.92,31.31l5.51-4.59a4.91,4.91,0,0,1,6.62.3Z"/>
                                                    <path className="cls-1" d="M14.37,5.53a8.68,8.68,0,0,1-8.68,8.68A8.62,8.62,0,0,1,1,12.84V9A8,8,0,0,1,9,1h4.05A8.63,8.63,0,0,1,14.37,5.53Z"/>
                                                </g>
                                            </g>
                                        </svg>
                                    </div>
                                )}
                                <Card.Title className="mb-4">{organization.name}</Card.Title>
                                <Card.Subtitle className="mb-2">{organization.description}</Card.Subtitle>
                                <Card.Footer>
                                    <Button variant="success" onClick={() => handleNavigate(organization.id)}>
                                        Go to
                                    </Button>
                                    <Button variant="secondary" onClick={() => navigate(`/edit-organization/${organization.id}`)}>
                                        Edit
                                    </Button>
                                    <Button variant="danger" onClick={() => handleModalShow(organization.id)}>
                                        Delete
                                    </Button>
                                </Card.Footer>
                            </Card.Body>
                        </Card>
                    ))}
                </div>
                <button className="add-circle-button" onClick={() => navigate(`/add-organization`)}>+</button>
                </>
            ) : (
                <div className="no-data-message">
                    <p>You have no organizations yet</p>
                    <button className="btn btn-primary" onClick={() => navigate(`/add-organization`)}>
                        Create your first organization
                    </button>
                </div>
            )}

            <Modal show={show} onHide={handleModalClose}>
                <Modal.Header closeButton>
                    <Modal.Title>Delete Organization</Modal.Title>
                </Modal.Header>
                <Modal.Body>Are you sure? All groups, employees, shifts, schedules associated with this organisation will be deleted. May be its easier to make some changes?</Modal.Body>
                <Modal.Footer>
                    <Button variant="secondary" onClick={handleModalClose}>
                        Close
                    </Button>
                    <Button variant="danger" onClick={() => handleDeleteClick()}>
                        Delete Organization
                    </Button>
                </Modal.Footer>
            </Modal>
        </div>
    );
};

export default Organizations;
