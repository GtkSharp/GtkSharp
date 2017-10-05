FROM debian:9

RUN apt-get update && \
    apt-get install -y git python3 python3-pip ninja-build mono-devel libgtk-3-dev

RUN pip3 install git+https://github.com/mesonbuild/meson/

ENV SOURCE_DIR="/source"

RUN mkdir -p "${SOURCE_DIR}"

COPY / "${SOURCE_DIR}"

CMD ["/source/.ci_build.sh"]
