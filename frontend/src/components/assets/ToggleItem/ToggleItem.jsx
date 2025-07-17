import './ToggleItem.css';

function ToggleItem () {
    return (
        <>
            <input type="checkbox" id="toggle" className="toggleCheckbox" />
            <label htmlFor="toggle" className='toggleContainer'>
            <div>This is a toggle</div>   
            <div>button</div>
            </label>
        </>
    )
}

export default ToggleItem;