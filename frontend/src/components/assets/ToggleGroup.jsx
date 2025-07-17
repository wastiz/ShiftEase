import { ToggleButtonGroup, ToggleButton } from "react-bootstrap";

function ToggleGroup({
                         name,
                         value,
                         onChange,
                         options = [],
                         idPrefix = "",
                         className = "",
                         variant = "light",
                     }) {
    return (
        <ToggleButtonGroup
            type="radio"
            name={name}
            value={value}
            onChange={onChange}
            className={className}
        >
            {options.map((opt) => (
                <ToggleButton
                    className="text-nowrap"
                    key={opt.value}
                    id={`${idPrefix}-${opt.value}`}
                    value={opt.value}
                    variant={variant}
                    onClick={opt.onClick}
                >
                    {opt.label}
                </ToggleButton>
            ))}
        </ToggleButtonGroup>
    );
}

export default ToggleGroup;
