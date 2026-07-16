import { useEffect, type ReactNode } from "react";
import { createPortal } from "react-dom";
import "./Modal.css";

interface ModalProps {
  title: string;
  onClose: () => void;
  children: ReactNode;
  footer?: ReactNode;
  size?: "sm" | "md" | "lg";
}

/** Generic, dependency-free modal dialog used across the admin screens. */
export function Modal({ title, onClose, children, footer, size = "md" }: ModalProps) {
  useEffect(() => {
    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === "Escape") {
        onClose();
      }
    };

    document.addEventListener("keydown", onKeyDown);
    document.body.style.overflow = "hidden";

    return () => {
      document.removeEventListener("keydown", onKeyDown);
      document.body.style.overflow = "";
    };
  }, [onClose]);

  return createPortal(
    <div
      className="itm-modal-overlay"
      onMouseDown={(event) => {
        if (event.target === event.currentTarget) {
          onClose();
        }
      }}
    >
      <div className={`itm-modal itm-modal--${size}`} role="dialog" aria-modal="true" aria-label={title}>
        <div className="itm-modal__header">
          <h3 className="itm-modal__title">{title}</h3>
          <button type="button" className="itm-modal__close" onClick={onClose} aria-label="Close">
            <i className="bi bi-x-lg" aria-hidden="true" />
          </button>
        </div>
        <div className="itm-modal__body">{children}</div>
        {footer && <div className="itm-modal__footer">{footer}</div>}
      </div>
    </div>,
    document.body
  );
}
