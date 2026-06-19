from __future__ import annotations

import sys

from PySide6.QtGui import QFont
from PySide6.QtWidgets import (
    QApplication,
    QHBoxLayout,
    QListWidget,
    QMainWindow,
    QStackedWidget,
    QWidget,
)

from odmr_console_core import RunBundle
from odmr_console_qt_artifact_review_page import ArtifactReviewPage
from odmr_console_qt_config_generator_page import ConfigGeneratorPage
from odmr_console_qt_resolve_page import ResolvePage
from odmr_console_qt_run_bundle_page import RunBundlePage
from odmr_console_qt_run_monitor_page import RunMonitorPage


class MainWindow(QMainWindow):
    def __init__(self) -> None:
        super().__init__()
        self.setWindowTitle("ODMR PySide6 Console")
        self.resize(1480, 920)
        self.setMinimumSize(1260, 780)

        central = QWidget()
        self.setCentralWidget(central)
        layout = QHBoxLayout(central)
        layout.setContentsMargins(12, 12, 12, 12)

        self.nav = QListWidget()
        self.nav.setFixedWidth(210)
        self.nav.addItems(["本次实验配置", "配置生成", "预检查 / 预计用时", "运行监控", "数据审查"])
        self.nav.setCurrentRow(0)
        layout.addWidget(self.nav)

        self.stack = QStackedWidget()
        layout.addWidget(self.stack, 1)

        self.run_bundle = RunBundlePage()
        self.config_generator = ConfigGeneratorPage()
        self.resolve_page = ResolvePage(self.run_bundle.bundle)
        self.monitor_page = RunMonitorPage(
            self.run_bundle.bundle,
            self.current_out_dir,
            self.run_bundle.validate_local,
            self.run_bundle.operator_metadata,
        )
        self.review_page = ArtifactReviewPage(self.current_out_dir)

        for page in [
            self.run_bundle,
            self.config_generator,
            self.resolve_page,
            self.monitor_page,
            self.review_page,
        ]:
            self.stack.addWidget(page)

        self.nav.currentRowChanged.connect(self.stack.setCurrentIndex)
        self.config_generator.bundle_generated.connect(self._bind_generated_bundle)
        self.monitor_page.active_out_dir_changed.connect(self.set_current_out_dir)

    def current_out_dir(self) -> str:
        return self.run_bundle.out_dir.text().strip()

    def set_current_out_dir(self, out_dir: str) -> None:
        self.run_bundle.set_out_dir(out_dir)
        self.review_page.set_current_out_dir(out_dir)

    def _bind_generated_bundle(self, bundle: RunBundle) -> None:
        self.run_bundle.set_bundle(bundle)
        self.run_bundle.make_new_out_dir()
        self.review_page.set_current_out_dir(self.current_out_dir())
        self.nav.setCurrentRow(0)


def main() -> int:
    app = QApplication(sys.argv)
    app.setApplicationName("ODMR PySide6 Console")

    font = QFont()
    font.setPointSize(12)
    app.setFont(font)
    app.setStyleSheet(
        """
        QGroupBox { font-weight: 600; margin-top: 14px; }
        QGroupBox::title { subcontrol-origin: margin; left: 8px; padding: 0 3px; }
        QLineEdit, QComboBox, QPlainTextEdit, QTableWidget { font-weight: 400; }
        QPushButton { min-height: 28px; padding-left: 10px; padding-right: 10px; }
        """
    )

    window = MainWindow()
    window.show()
    return app.exec()


if __name__ == "__main__":
    raise SystemExit(main())
