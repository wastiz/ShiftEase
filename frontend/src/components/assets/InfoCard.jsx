import { Card, Button } from "react-bootstrap";

function InfoCard({
                      title,
                      subtitle,
                      image,
                      text,
                      content,
                      actions = [],
                      style = {},
                      className = "",
                  }) {
    return (
        <Card className={className} style={style}>
            <Card.Body>
                {image && <Card.Img src={image} />}
                {title && <Card.Title className="mb-4">{title}</Card.Title>}
                {subtitle && <Card.Subtitle>{subtitle}</Card.Subtitle>}
                {text && <Card.Text>{text}</Card.Text>}
                {content}
                {actions.length > 0 && (
                    <Card.Footer>
                        {actions.map(({ label, variant, onClick, icon }, index) => (
                            <Button key={index} variant={variant || "primary"} onClick={onClick}>
                                {icon && <span>{icon}</span>}
                                {label}
                            </Button>
                        ))}
                    </Card.Footer>
                )}
            </Card.Body>
        </Card>
    );
}

export default InfoCard;
