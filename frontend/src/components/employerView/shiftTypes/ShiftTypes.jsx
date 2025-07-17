import './ShiftTypes.css';
import { useState, useEffect } from "react";
import AsideModal from '../../assets/asideModal/AsideModal.jsx';
import ShiftAddForm from './ShiftTypeAddForm.jsx';
import api from '../../../api.js';
import Cookies from "js-cookie";
import InfoCard from "../../assets/InfoCard.jsx";
import LoadingSpinner from "../../assets/LoadingSpinner.jsx";

function ShiftTypes() {
    const api_route = import.meta.env.VITE_SERVER_API;
    const token = Cookies.get('auth_token');
    const [shifts, setShifts] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [modalOn, setModalOn] = useState(false);
    const [selectedShift, setSelectedShift] = useState(null);

    const fetchShifts = async () => {
        try {
            setLoading(true);
            const response = await api.get(`${api_route}/ShiftType/organization`, {
                headers: {
                    "Authorization": `Bearer ${token}`,
                    "Content-Type": "application/json"
                }
            });

            setShifts(response.data);
        } catch (err) {
            setError(err.message);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchShifts();
    }, [token]);

    const handleEditClick = (shift) => {
        setSelectedShift(shift);
        setModalOn(true);
    };

    const handleAddClick = () => {
        setSelectedShift(null);
        setModalOn(true);
    };

    const handleClose = () => {
        setModalOn(false);
        setSelectedShift(null);
    };

    const handleCreate = (newShift) => {
        setShifts(prev => [...prev, newShift]);
    };

    const handleUpdate = (updatedShift) => {
        setShifts(prev => prev.map(shift =>
            shift.id === updatedShift.id ? updatedShift : shift
        ));
    };

    const handleDelete = (deletedId) => {
        setShifts(prev => prev.filter(shift => shift.id !== deletedId));
    };

    return (
        <div className="p-4">
            <div className='d-flex flex-row justify-content-between mb-5'>
                <h2>Shifts Types</h2>
            </div>

            <button className="add-circle-button" onClick={handleAddClick}>+</button>

            {loading ? <LoadingSpinner visible={loading} /> : error ? <p>Error: {error}</p> : (
                <div className="shift-type-container">
                    {shifts.length === 0 ? (
                        <p>No shifts types. Create one</p>
                    ) : (
                        shifts.map(shift => (
                            <InfoCard
                                key={shift.id}
                                style={{ borderColor: shift.color }}
                                title={shift.name}
                                subtitle={
                                    <>
                                        <p className="mb-2">Employee needed: {shift.employeeNeeded}</p>
                                        <p className="mb-2">Start time: {shift.startTime}</p>
                                        <p className="mb-2">End time: {shift.endTime}</p>
                                    </>
                                }
                                actions={[
                                    {
                                        label: "Edit",
                                        variant: "secondary",
                                        onClick: () => handleEditClick(shift)
                                    }
                                ]}
                            />
                        ))
                    )}
                </div>
            )}

            <AsideModal modalOn={modalOn} onClose={handleClose} name='Add Shift Type'>
                <ShiftAddForm 
                    modalOn={modalOn} 
                    onClose={handleClose} 
                    shiftTypeData={selectedShift}
                    onCreate={handleCreate}
                    onUpdate={handleUpdate}
                    onDelete={handleDelete}
                />
            </AsideModal>
        </div>
    );
}

export default ShiftTypes;
