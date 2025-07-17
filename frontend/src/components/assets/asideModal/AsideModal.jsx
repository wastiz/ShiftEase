import './AsideModal.css';

function AsideModal({ modalOn, onClose, name, children }) {
    return (
        <div className={`aside-modal ${modalOn ? 'open' : ''}`}>
            <div className='aside-modal-header'>
                <h4>{name}</h4>
                <button className="btn btn-primary" onClick={onClose}>&times;</button>
            </div>
            <div className="aside-modal-content">
                {children}
            </div>
        </div>
    );
}

export default AsideModal;